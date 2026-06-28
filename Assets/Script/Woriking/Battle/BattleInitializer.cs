using UnityEngine;
using UnityEngine.UI;
public class BattleInitalizer : MonoBehaviour
{
    public static BattleInitalizer instance;

    [SerializeField] private BaseStageController stageController;
    public Image enemyImage;
    public Image playerImage;
    public GameObject spawnEnemy;
    private EnemyCharacter enemyCharacter;
    private PlayerCharacter playerCharacter;

    public EnemyCharacter EnemyCharacter => enemyCharacter;
    public PlayerCharacter PlayerCharacter => playerCharacter;

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
            stageController.PlayIntroAnim();

            var enemyPrefab = BattleDataManager.instance?.GetEnemyPrefab();
            if (enemyPrefab != null)
            {
                var bossData = BattleDataManager.instance.currentEnemyData as BossDataSo;
                enemyImage.gameObject.SetActive(false);
                spawnEnemy = Instantiate(enemyPrefab);
                spawnEnemy.transform.position = bossData.spawnPosition;
                spawnEnemy.transform.localScale = bossData.bossScale;
                enemyCharacter = spawnEnemy.GetComponent<EnemyCharacter>();
                enemyCharacter.SetAlpha(0f);

                
            }
            else if (enemyImage != null)
            {
                enemyImage.gameObject.SetActive(true);
                enemyImage.sprite = BattleDataManager.instance?.GetEnemyImage();
                enemyCharacter = enemyImage.GetComponent<EnemyCharacter>();
                enemyCharacter.SetAlpha(0f);
            }

            if (playerImage != null)
            {
                playerImage.gameObject.SetActive(true);
                playerImage.sprite = ResourceManager.instance?.PlayerImage;
                playerCharacter = playerImage.GetComponent<PlayerCharacter>();
                playerCharacter.SetAlpha(0f);
            }
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
            enemyCharacter?.SubscribeToBattleEvents(BattleManager.instance.EventBus);
        }
    }

    public void CompleteBattle(bool isSuccess)
    {
        if (UiController.instance != null)
            UiController.instance.SetRollBtnInteractable(false);

        if (BattleManager.instance != null)
            enemyCharacter?.UnsubscribeFromBattleEvents(BattleManager.instance.EventBus);

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
            ResourceManager.instance.AddGold(BattleDataManager.instance.GetGoldReward());
            RewardPanelUI.instance?.Show(BattleDataManager.instance.currentRewardData);
        }
        else
        {
            GameManager.instance.HandleGameOver();
        }
    }
}