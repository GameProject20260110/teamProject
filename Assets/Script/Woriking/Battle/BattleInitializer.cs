using System.Threading.Tasks;
using UnityEngine;
using VContainer;

public class BattleInitalizer : MonoBehaviour
{
    [SerializeField] private BaseStageController stageController;
    [SerializeField] private CardRevealAnimator ClearAnim;

    public GameObject spawnPlayer;
    public GameObject spawnEnemy;

    private EnemyCharacter enemyCharacter;
    private PlayerCharacter playerCharacter;

    public EnemyCharacter EnemyCharacter => enemyCharacter;
    public PlayerCharacter PlayerCharacter => playerCharacter;

    private ResourceManager _resourceManager;
    private BattleDataManager _battleDataManager;

    private DeckManager _deckManager;
    private EnemyDeckHandler _enemyDeckHandler;
    private EnemyDeathSequence _enemyDeathSequence;

    [Inject]
    public void Construct(
        ResourceManager resourceManager,
        BattleDataManager battleDataManager,

        DeckManager deckManager,
        EnemyDeckHandler enemyDeckHandler,
        EnemyDeathSequence enemyDeathSequence)
    {
        _resourceManager = resourceManager;
        _battleDataManager = battleDataManager;
        _deckManager = deckManager;
        _enemyDeckHandler = enemyDeckHandler;
        _enemyDeathSequence = enemyDeathSequence;
    }

    private void Start()
    {
        StartBattle();
    }

    public void SetSpawnPlayer(GameObject player)
    {
        spawnPlayer = player;
        playerCharacter = spawnPlayer.GetComponentInChildren<PlayerCharacter>();
        BattleManager.Instance.SetPlayerTransform(spawnPlayer.transform);
        PlayerCharacter?.SubscribeToBattleEvents(BattleManager.Instance.EventBus);
    }

    public void SetSpawnEnemy(GameObject enemy)
    {
        spawnEnemy = enemy;
        enemyCharacter = spawnEnemy.GetComponentInChildren<EnemyCharacter>();
        BattleManager.Instance.SetEnemyTransform(spawnEnemy.transform);
        _enemyDeathSequence?.SetupEnemy(enemy);
        enemyCharacter?.SubscribeToBattleEvents(BattleManager.Instance.EventBus);
    }

    public void StartBattle()
    {
        stageController.PlayIntroAnim();

        GameManager.Instance?.InitializeRoundData();

        if (UiController.Instance != null)
        {
            UiController.Instance.HideAllPanels();
            UiController.Instance.SetRollBtnInteractable(true);
            UiController.Instance.SetConfirmBtnInteratable(false);
            UiController.Instance.ResetItemCards();
        }

        if (_deckManager != null)
        {
            _deckManager.InitializeDeck();
            _deckManager.DrawDice();
        }

        _enemyDeckHandler?.SetupEnemyDice();
        BattleManager.Instance?.InitializeBattle();
    }

    public async Task CompleteBattleAsync(bool isSuccess)
    {
        UiController.Instance?.SetRollBtnInteractable(false);
        PlayerCharacter?.UnsubscribeFromBattleEvents(BattleManager.Instance.EventBus);
        enemyCharacter?.UnsubscribeFromBattleEvents(BattleManager.Instance.EventBus);

        int currentHP = _resourceManager != null ? _resourceManager.heart : 0;

        if (isSuccess)
        {
            if (_battleDataManager?.isBossBattle == true)
            {
                MapManager.Instance?.ClearMapSave();
                _battleDataManager?.Clear();
            }
            _resourceManager.AddGold(_battleDataManager.GetGoldReward());
            ClearAnim.gameObject.SetActive(true);
            await ClearAnim.Reveal();
        }
        else
        {
            GameManager.Instance.HandleGameOver();
        }
    }
}