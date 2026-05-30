using UnityEngine;
using UnityEngine.UI;
public class BattleInitalizer : MonoBehaviour
{
    public static BattleInitalizer instance;

    [SerializeField] private RoundController roundEffect;
    public Image enemyImage;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartBattle();
    }

    public void StartBattle()
    {       
        {
            roundEffect.PlayIntroAnim();

            if (enemyImage != null)
            {
                enemyImage.sprite = BattleDataManager.instance?.GetEnemyImage();
            }

            // 보스전만 기믹
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.InitializeRoundData();
        }

        if (UiController.instance != null)
        {
            UiController.instance.HideAllPanels();
            UiController.instance.SetRollBtnInteractable(true);
            UiController.instance.SetConfirmBtnInteratable(false);
            UiController.instance.SetRollButtonToRoll();
            UiController.instance.ResetItemCards();
        }

        if (VisualManager.instance != null)
            VisualManager.instance.ResetDiceColors(GameManager.instance.diceManager.GetAllDice());


        if (DeckManager.instance != null)
        {
            DeckManager.instance.InitializeDeck();
            DeckManager.instance.DrawDice();
        }

        if (EnemyDeckManager.instance != null)
        {
            EnemyDeckManager.instance.InitializeDeck();
            EnemyDeckManager.instance.DrawEnemyDice();
        }

        if (BattleManager.instance != null)
        {
            BattleManager.instance.InitializeBattle();
        }
    }

    public void CompleteBattle(bool isSuccess)
    {
        if (UiController.instance != null)
            UiController.instance.SetRollBtnInteractable(false);

        //if (PlayerManager.instance != null)
        //{
        //    foreach (var item in PlayerManager.instance.items)
        //    {
        //        if (item == null) continue;
        //        item.RoundEnd();
        //    }
        //}
        //if (PlayerManager.instance.tempExtraSlotsCount > 0)
        //{
        //    bool[] slots = PlayerManager.instance.SpecialSlots;
        //    int remove = 0;
        //    for (int i = slots.Length - 1; i >= 0 && remove < PlayerManager.instance.tempExtraSlotsCount; i--)
        //    {
        //        if (slots[i])
        //        {
        //            slots[i] = false;
        //            remove++;
        //        }
        //    }
        //    PlayerManager.instance.tempExtraSlotsCount = 0;
        //}

        int currentHP = PlayerManager.instance != null ? PlayerManager.instance.heart : 0;

        // 클리어 시 선택 보상

        if (isSuccess)
        {
            int reward = BattleDataManager.instance?.GetGoldReward() ?? 10;
            GameManager.instance.AddGold(reward);

            // 보스전 클리어 후 맵 데이터 초기화
            if (BattleDataManager.instance?.isBossBattle == true)
            {
                MapManager.instance?.ClearMapSave();
                BattleDataManager.instance?.Clear();
            }
            UiController.instance.ShowResultPanel(true, currentHP);
        }
        else
        {
            GameManager.instance.HandleGameOver();
        }
        GimmickManager.instance.ClearGimmick();
    }

    public void GoNextRound()
    {
        PlayerManager.instance.Save();
    }
}


