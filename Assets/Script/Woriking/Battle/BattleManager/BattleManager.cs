using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;
    [Header("전투 데이터")]
    [SerializeField] private PlayerBattleData playerData;
    [SerializeField] private EnemyBattleData enemyData;
    [SerializeField] private PlayerData playerSO;

    [SerializeField] private Transform Enemytrans;
    [SerializeField] private Transform Playertrans;
    private BaseEnemyData enemyInfo;

    [Header("reference")]
    [SerializeField] private BattleUI battleUI;
    [SerializeField] private EnemyDeathSequence enemyDeathSequence;

    public bool isPlayerTurn = true;
    public bool isBattleActive = false;
    public int currentTurn = 1;
    public GameObject EnemySkillPrefab;

    private CancellationTokenSource _battleCts;

    private BattleEventBus _eventBus;
    private BattleSaveHandler _saveHandler;
    private TurnController _turnController;
    private BattleUIBinder _uiBinder;
    private BattleContextFactory _contextFactory;

    private readonly BattleDiceRegistry _diceRegistry = new BattleDiceRegistry();
    private readonly BattleSubscriptionManager _subscriptionManager = new BattleSubscriptionManager();

    public CancellationToken BattleToken => _battleCts.Token;
    public BattleEventBus EventBus => _eventBus;
    public PlayerBattleData PlayerData => playerData;
    public EnemyBattleData EnemyData => enemyData;

    public List<Dice> AttackDices => _diceRegistry.AttackDices;
    public List<Dice> DefenseDices => _diceRegistry.DefenseDices;
    public List<Dice> AttackEnemyDices => _diceRegistry.AttackEnemyDices;
    public List<Dice> DefenseEnemyDices => _diceRegistry.DefenseEnemyDices;

    private void OnDestroy()
    {
        _battleCts?.Cancel();
        _battleCts?.Dispose();
    }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        _saveHandler = new BattleSaveHandler(playerData, enemyData, battleUI, playerSO);
    }

    private void Start()
    {
        LoadBattleData();
    }

    public void ShowBonusDamageText(int damage) => battleUI.ShowBonusDamageText(damage);
    public void ShowHealText(int amount) => battleUI.ShowHealText(amount);

    #region Context

    public BattleContext CreateCtx(bool isPlayer = true) => _contextFactory.CreateCtx(isPlayer);

    public DiceContext CreateDiceCtx(bool isPlayer, Dice dice, List<Dice> attack, List<Dice> defense)
        => _contextFactory.CreateDiceCtx(isPlayer, dice, attack, defense);

    #endregion

    #region Init

    public void SetPlayerTransform(Transform playerTransform)
    {
        Playertrans = playerTransform;
    }

    public void SetEnemyTransform(Transform enemyTransform)
    {
        Enemytrans = enemyTransform;
    }

    public void InitializeBattle()
    {
        if (BattleInitalizer.instance == null) return;

        enemyInfo = BattleDataManager.instance.currentEnemyData;

        _battleCts?.Cancel();
        _battleCts?.Dispose();
        _battleCts = new CancellationTokenSource();

        _eventBus = new BattleEventBus();

        enemyData.Initialize(enemyInfo, _eventBus);
        playerData.Initialize(playerSO, _eventBus);

        isPlayerTurn = false;
        isBattleActive = true;
        currentTurn = 1;

        _contextFactory = new BattleContextFactory(
            playerData, enemyData, () => Playertrans, () => Enemytrans,
            _eventBus, _battleCts.Token, () => currentTurn);

        // UI 구독
        _uiBinder = new BattleUIBinder(battleUI, playerData, enemyData);
        _uiBinder.Subscribe(_eventBus);

        // 아이템 / 보스 기믹 구독
        _subscriptionManager.Subscribe(_eventBus, enemyInfo);

        _turnController = new TurnController(this);

        battleUI.UpdateEnemyHP(enemyData.CurrentHP, enemyData.MaxHp);
        battleUI.UpdatePlayerHP(playerData.CurrentHP, playerData.MaxHp);

        _saveHandler.Save(isPlayerTurn, isBattleActive, currentTurn);
    }

    public UniTask RunOneTurnCycle() => _turnController.RunOneTurnCycle();

    public void TriggerFirstTurnStart()
    {
        _eventBus.TriggerTurnStart(CreateCtx());
    }

    #endregion

    #region battleEnd

    public async UniTask HandleBattleEnd(bool isSuccess)
    {
        if (isSuccess)
        {
            _eventBus.TriggerEnemyDead(CreateCtx());
            await enemyDeathSequence.PlayDeathSequence(Enemytrans.position);
        }

        OnBattleEnd();
        await BattleInitalizer.instance.CompleteBattleAsync(isSuccess);
    }

    private void OnBattleEnd()
    {
        currentTurn = 1;
        isBattleActive = false;

        playerData.ResetShield();
        enemyData.ResetShield();
        UpdateShieldUI();

        _battleCts?.Cancel();
        _saveHandler.Delete();

        _uiBinder?.Unsubscribe(_eventBus);
        _subscriptionManager.Unsubscribe(_eventBus, enemyInfo);
    }

    #endregion

    #region TurnController_Helper

    public void UpdateShieldUI()
    {
        battleUI.UpdatePlayerShield(playerData.CurrentShield);
        battleUI.UpdateEnemyShield(enemyData.CurrentShield);
    }

    public async UniTask UpdateTurnUI()
    {
        await battleUI.UpdateCurrentTurn(currentTurn);
    }

    public void ResetAllDiceVFX() => _diceRegistry.ResetAllVFX();

    public void ClearEnemyDices() => _diceRegistry.ClearEnemyDices();

    #endregion

    #region etc..

    public void SetDiceInfo(List<Dice> attackDices, List<Dice> defenseDices)
        => _diceRegistry.SetPlayerDice(attackDices, defenseDices);

    public void SetEnemyAttackDice(Dice attackEnemyDice)
        => _diceRegistry.AddEnemyAttackDice(attackEnemyDice);

    public void SetEnemyDefenseDice(Dice defenseEnemyDice)
        => _diceRegistry.AddEnemyDefenseDice(defenseEnemyDice);

    public void UseItem(BattleItemSo item)
    {
        if (!isBattleActive) return;

        var diceCtx = new DiceContext { battle = CreateCtx() };
        item.OnUse(diceCtx);

        if (item.isConsumable)
        {
            ItemManager.instance.items.Remove(item);
            UiController.instance.RefreshInventory();
        }
    }

    public void SaveBattleData()
        => _saveHandler.Save(isPlayerTurn, isBattleActive, currentTurn);

    public void LoadBattleData()
    {
        _saveHandler = new BattleSaveHandler(playerData, enemyData, battleUI, playerSO);
        var data = _saveHandler.Load();
        if (data == null) return;

        playerData.Initialize(playerSO, data.playerCurrentHP, _eventBus);
        enemyData.Initialize(BattleDataManager.instance.currentEnemyData, data.enemyMaxHP, _eventBus);

        isPlayerTurn = data.isPlayerTurn;
        isBattleActive = data.isBattleActive;
        currentTurn = data.currentBattleRound;

        battleUI.UpdatePlayerHP(playerData.CurrentHP, playerData.MaxHp);
        battleUI.UpdateEnemyHP(enemyData.CurrentHP, enemyData.MaxHp);

        Debug.Log("전투 데이터 로드 완료");
    }

    public void DeleteBattleData() => _saveHandler.Delete();

    #endregion
}