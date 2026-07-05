using System.Threading.Tasks;
using UnityEngine;
public class BattleInitalizer : MonoBehaviour
{
    public static BattleInitalizer instance;
    [SerializeField] private BaseStageController stageController;
    [SerializeField] private CardRevealAnimator ClearAnim;

    public GameObject spawnPlayer;
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

    public void SetSpawnPlayer(GameObject player)
    {
        spawnPlayer = player;
        playerCharacter = spawnPlayer.GetComponentInChildren<PlayerCharacter>();
        BattleManager.instance.SetPlayerTransform(spawnPlayer.transform);

        if (BattleManager.instance != null)
            PlayerCharacter?.SubscribeToBattleEvents(BattleManager.instance.EventBus);
    }

    public void SetSpawnEnemy(GameObject enemy)
    {
        spawnEnemy = enemy;
        enemyCharacter = spawnEnemy.GetComponentInChildren<EnemyCharacter>();
        BattleManager.instance.SetEnemyTransform(spawnEnemy.transform);
        EnemyDeathSequence.instance?.SetupEnemy(enemy);

        if (BattleManager.instance != null)
            enemyCharacter?.SubscribeToBattleEvents(BattleManager.instance.EventBus);
    }

    public void StartBattle()
    {             
        stageController.PlayIntroAnim();
 
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

    public async Task CompleteBattleAsync(bool isSuccess)
    {
        if (UiController.instance != null)
            UiController.instance.SetRollBtnInteractable(false);

        if (BattleManager.instance != null)
            PlayerCharacter?.UnsubscribeFromBattleEvents(BattleManager.instance.EventBus);

        if (BattleManager.instance != null)
            enemyCharacter?.UnsubscribeFromBattleEvents(BattleManager.instance.EventBus);

        //int currentHP = ResourceManager.instance != null ? ResourceManager.instance.heart : 0;

        // 클리어 시 선택 보상

        if (isSuccess)
        {
            int goldReward = BattleDataManager.instance.GetGoldReward();
            var rewardData = BattleDataManager.instance.currentRewardData;
            bool wasBossBattle = BattleDataManager.instance?.isBossBattle == true;

            if(wasBossBattle)
            {
                MapManager.instance?.ClearMapSave();
                BattleDataManager.instance?.Clear();
            }

            ResourceManager.instance.AddGold(goldReward);

            if(wasBossBattle)
            {
                UiController.instance.ShowGameOverPanel(true);
            }
            else
            {
                RewardPanelUI.instance?.Show(rewardData);
            }

            //// 보스전 클리어 후 맵 데이터 초기화
            //if (BattleDataManager.instance?.isBossBattle == true)
            //{
            //    MapManager.instance?.ClearMapSave();
            //    BattleDataManager.instance?.Clear();
            //}
            //ResourceManager.instance.AddGold(BattleDataManager.instance.GetGoldReward());
            //ClearAnim.gameObject.SetActive(true);
            //await ClearAnim.Reveal();
        }
        else
        {
            GameManager.instance.HandleGameOver();
        }
    }
}