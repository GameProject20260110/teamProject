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
            UiController.instance.ResetItemCards();
        }

        if (DeckManager.instance != null)
        {
            DeckManager.instance.InitializeDeck();
            DeckManager.instance.DrawDice();
        }

        if (EnemyDeckHandler.instance != null)
        {
            EnemyDeckHandler.instance.SetupEnemyDice();
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

        int currentHP = ResourceManager.instance != null ? ResourceManager.instance.heart : 0;

        // 클리어 시 선택 보상

        if (isSuccess)
        {
            // 보스전 클리어 후 맵 데이터 초기화
            if (BattleDataManager.instance?.isBossBattle == true)
            {
                MapManager.instance?.ClearMapSave();
                BattleDataManager.instance?.Clear();
            }

            RewardPanelUI.instance?.Show(BattleDataManager.instance.currentRewardData);
        }
        else
        {
            GameManager.instance.HandleGameOver();
        }
    }
}