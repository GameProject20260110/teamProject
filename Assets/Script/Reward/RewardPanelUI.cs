using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RewardPanelUI : MonoBehaviour
{
    public static RewardPanelUI instance;

    [Header("UI")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject rewardCardPrefab;
    [SerializeField] private Button skipButton;

    private List<GameObject> _spawnedCards = new List<GameObject>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        rewardPanel.SetActive(false);

        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipButton);
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
        SpawnCards(rewards);
    }

    private void SpawnCards(List<RewardData> rewards)
    {
        foreach (var card in _spawnedCards)
            Destroy(card);
        _spawnedCards.Clear();

        foreach(var reward in rewards)
        {
            // æ∆¿Ã≈€ ƒ´µÂ æ∆¿Ã≈€ πÃ∏Æ ∞·¡§
            BattleItemSo preSelectedItem = null;
            if ((reward.rewardType == RewardType.ActiveItem || reward.rewardType == RewardType.PassiveItem) && reward.itemTable != null)
                preSelectedItem = reward.itemTable.GetRandomItem();
            GameObject cardObj = Instantiate(rewardCardPrefab, cardContainer);
            RewardCardUI cardUI = cardObj.GetComponent<RewardCardUI>();
            cardUI.SetUp(reward, preSelectedItem, (r, item) => OnRewardSelected(r, item));
            _spawnedCards.Add(cardObj);
        }
    }

    private void OnRewardSelected(RewardData reward, BattleItemSo preSelectedItem)
    {
        ApplyReward(reward, preSelectedItem);
        Hide();
        GoToMap();
    }

    private void ApplyReward(RewardData reward, BattleItemSo preSelecteItem = null)
    {
        switch(reward.rewardType)
        {
            case RewardType.Gold:
                if(ResourceManager.instance != null)
                {
                    ResourceManager.instance.gold += reward.goldAmount;
                    ResourceManager.instance.Save();
                    Debug.Log($"{reward.goldAmount} ∞ÒµÂ »πµÊ");
                }
                break;
            case RewardType.HpPotion:
                if(ResourceManager.instance != null )
                {
                    ResourceManager.instance.heart += reward.healAmount;
                    ResourceManager.instance.Save();
                    Debug.Log($"{reward.healAmount} √º∑¬ »∏∫π");
                }
                break;
            case RewardType.Dice:
                if(reward.dice != null)
                {
                    PlayerDeck.instance?.AddDice(reward.dice);
                    Debug.Log($"{reward.dice.name} ¡÷ªÁ¿ß »πµÊ");
                }
                break;
            case RewardType.PassiveItem:
            case RewardType.ActiveItem:
                if (preSelecteItem != null)
                {
                    ItemManager.instance?.items.Add(preSelecteItem);
                    ItemManager.instance?.Save();
                    Debug.Log($"{preSelecteItem.itemName} æ∆¿Ã≈€ »πµÊ");
                }
                break;
        }
    }

    private void OnSkipButton()
    {
        Hide();
        GoToMap();
    }

    private void Hide()
    {
        rewardPanel.SetActive(false);
    }

    private void GoToMap()
    {
        SceneController.instance?.LoadMapScene();
    }
}
