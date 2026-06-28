using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;

public class RewardPanelUI : MonoBehaviour
{
    public static RewardPanelUI instance;

    [Header("UI")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private GameObject background;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private RewardIntroAnimator introAnimator;

    [Header("카드 슬롯")]
    [SerializeField] private List<RectTransform> cardSlots;
    [SerializeField] private GameObject rewardCardPrefab;

    [Header("주사위 보상")]
    [SerializeField] private GameObject diceRewardContainer;
    [SerializeField] private Image diceImage;
    [SerializeField] private TextMeshProUGUI diceNameText;
    [SerializeField] private DiceRewardAnimator diceRewardAnimator;

    [Header("버튼")]
    [SerializeField] private Button acquireButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button diceRewardSkipButton;

    private List<GameObject> _spawnedCards = new List<GameObject>();
    private DiceData _preSelectedDice;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        rewardPanel.SetActive(false);
        diceRewardContainer.SetActive(false);

        skipButton?.onClick.AddListener(OnSkipButton);
        acquireButton?.onClick.AddListener(OnAcquireButton);
        diceRewardSkipButton?.onClick.AddListener(OnSkipButton);
    }

    public void Show(RewardDataSo rewardData)
    {
        if (rewardData == null || rewardData.rewardPool == null || rewardData.rewardPool.Count == 0)
        {
            GoToMap();
            return;
        }

        List<RewardData> rewards = rewardData.GetRewards(rewardData.rewardCount);

        if(rewards == null || rewards.Count == 0)
        {
            GoToMap();
            return;
        }

        rewardPanel.SetActive(true);
        background.SetActive(true);
        skipButton.gameObject.SetActive(false);

        List<RewardCardUI> cards = SpawnCards(rewards);
        introAnimator.PlayIntro(cards).Forget();

    }

    private List<RewardCardUI> SpawnCards(List<RewardData> rewards)
    {
        foreach (var card in _spawnedCards)
            Destroy(card);
        _spawnedCards.Clear();

        List<RewardCardUI> cards = new List<RewardCardUI>();


        for(int i = 0; i < rewards.Count; i++)
        {
            if (i >= cardSlots.Count) break;

            var reward = rewards[i];

            // 아이템 카드 아이템 미리 결정
            BattleItemSo preSelectedItem = null;
            if ((reward.rewardType == RewardType.ActiveItem || reward.rewardType == RewardType.PassiveItem) && reward.itemTable != null)
                preSelectedItem = reward.itemTable.GetRandomItem();

            DiceData preSelectedDice = null;
            if (reward.rewardType == RewardType.Dice && reward.diceTable != null)
                preSelectedDice = reward.diceTable.GetRandomDice();

            GameObject cardObj = Instantiate(rewardCardPrefab, cardSlots[i], false);
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            cardRect.anchoredPosition = Vector2.zero;
            

            RewardCardUI cardUI = cardObj.GetComponent<RewardCardUI>();
            cardUI.SetUp(reward, preSelectedItem, preSelectedDice, (r, item, dice) => OnRewardSelected(r, item, dice));
            _spawnedCards.Add(cardObj);
            cards.Add(cardUI);
        }
        return cards;
    }

    private void OnRewardSelected(RewardData reward, BattleItemSo preSelectedItem, DiceData preSelectedDice)
    {
        if(reward.rewardType == RewardType.Dice)
        {
            _preSelectedDice = preSelectedDice;
            ShowDiceReward(preSelectedDice).Forget();
        }
        else
        {
            ApplyReward(reward, preSelectedItem, null);
            Hide();
            GoToMap();
        }
    }

    private async UniTask ShowDiceReward(DiceData dice)
    {
        cardContainer.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        if (diceImage != null && dice != null)
            diceImage.sprite = dice.skin?.GetSprite(1);

        if(diceNameText != null && dice != null) 
            diceNameText.text = dice.abilityName;

        diceRewardContainer.SetActive(true);

        await diceRewardAnimator.PlayAnimation();
    }

    private void ApplyReward(RewardData reward, BattleItemSo preSelecteItem = null, DiceData preSelectedDice = null)
    {
        if(preSelectedDice != null)
        {
            bool replaced = PlayerDeck.instance.ReplaceDefaultDice(preSelectedDice);
            if (!replaced)
                PlayerDeck.instance.AddDice(preSelectedDice);
            Debug.Log($"{preSelectedDice.name} 주사위 획득");
            return;
        }

        switch(reward.rewardType)
        {
            case RewardType.Gold:
                if(ResourceManager.instance != null)
                {
                    ResourceManager.instance.AddGold(reward.goldAmount);
                }
                break;
            case RewardType.HpPotion:
                if(ResourceManager.instance != null )
                {
                    ResourceManager.instance.heart += reward.healAmount;
                    ResourceManager.instance.Save();
                    Debug.Log($"{reward.healAmount} 체력 회복");
                }
                break;
            case RewardType.Dice:
                if(reward.dice != null)
                {
                    PlayerDeck.instance?.AddDice(reward.dice);
                    Debug.Log($"{reward.dice.name} 주사위 획득");
                }
                break;
            case RewardType.PassiveItem:
            case RewardType.ActiveItem:
                if (preSelecteItem != null)
                {
                    ItemManager.instance?.AddItem(preSelecteItem);
                    Debug.Log($"{preSelecteItem.itemName} 아이템 획득");
                }
                break;
        }
    }

    private void OnAcquireButton()
    {
        diceRewardAnimator.StopFloating();
        ApplyReward(null, null, _preSelectedDice);
        Hide();
        GoToMap();
    }


    private void OnSkipButton()
    {
        diceRewardAnimator.StopFloating();
        Hide();
        GoToMap();
    }

    private void Hide()
    {
        rewardPanel.SetActive(false);
        background.SetActive(false);
        diceRewardContainer.SetActive(false);
        cardContainer.gameObject.SetActive(true);
        skipButton.gameObject.SetActive(true);
    }

    private void GoToMap()
    {
        SceneController.instance?.LoadMapScene();
    }
}
