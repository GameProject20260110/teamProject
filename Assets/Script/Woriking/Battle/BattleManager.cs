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

    private List<Dice> _attackDices = new List<Dice>();
    private List<Dice> _defenseDices = new List<Dice>();
    private List<Dice> _attackEnemyDices = new List<Dice>();
    private List<Dice> _defenseEnemyDices = new List<Dice>();

    private BattleEventBus _eventBus;
    private BattleSaveHandler _saveHandler;
    private TurnController _turnController;

    public CancellationToken BattleToken => _battleCts.Token;
    public BattleEventBus EventBus => _eventBus;
    public PlayerBattleData PlayerData => playerData;
    public EnemyBattleData EnemyData => enemyData;

    public List<Dice> AttackDices => _attackDices;
    public List<Dice> DefenseDices => _defenseDices;
    public List<Dice> AttackEnemyDices => _attackEnemyDices;
    public List<Dice> DefenseEnemyDices => _defenseEnemyDices;

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

    public BattleContext CreateCtx(bool isPlayer = true)
    {
        return new BattleContext
        {
            Player = playerData,
            Enemy = enemyData,
            IsPlayer = isPlayer,
            EventBus = _eventBus,
            CancellationToken = _battleCts.Token,
            GetCurrentTurn = () => currentTurn,
            Positions = new BattlePositions
            {
                EnemyPosition = Enemytrans.position,
                PlayerPosition = Playertrans.position
            }
        };
    }

    public DiceContext CreateDiceCtx(bool isPlayer, Dice dice, List<Dice> attack, List<Dice> defense)
    {
        return new DiceContext
        {
            battle = CreateCtx(isPlayer),
            baseDamage = dice.MyState.modifiedValue,
            diceState = dice.MyState,
            dices = new BattleDices { attackDices = attack, defenseDices = defense }
        };
    }

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

        enemyData.Initialize(enemyInfo);
        playerData.Initialize(playerSO);

        isPlayerTurn = false;
        isBattleActive = true;
        currentTurn = 1;

        _eventBus = new BattleEventBus();

        // UI 구독
        _eventBus.OnPlayerHeal += HandlePlayerHeal;
        _eventBus.OnEnemyHeal += HandleEnemyHeal;
        _eventBus.OnHitEnemy += HandleHitEnemy;
        _eventBus.OnPlayerDefend += HandlePlayerDefend;
        _eventBus.OnPlayerHit += HandleHitPlayer;
        _eventBus.OnEnemyDefend += HandleEnemyDefend;

        // 아이템 구독
        foreach (var item in ItemManager.instance.items)
            item.OnEquip(_eventBus);
        foreach (var item in ItemManager.instance.artifacts)
            item.OnEquip(_eventBus);

        // 보스 기믹 구독
        if (enemyInfo is BossDataSo bossData)
            bossData.RegisterAllGimmicks(_eventBus);

        _turnController = new TurnController(this);

        battleUI.UpdateEnemyHP(enemyData.CurrentHP, enemyData.MaxHp);
        battleUI.UpdatePlayerHP(playerData.CurrentHP, playerData.MaxHp);

        _saveHandler.Save(isPlayerTurn, isBattleActive, currentTurn);
    }

    public UniTask RunOneTurnCycle() => _turnController.RunOneTurnCycle();

    public void TriggerFirstTurnStart()
    {
        // 1턴 시작 이벤트
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
        BattleInitalizer.instance.CompleteBattle(isSuccess);
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

        // UI 구독 해제
        _eventBus.OnPlayerHeal -= HandlePlayerHeal;
        _eventBus.OnEnemyHeal -= HandleEnemyHeal;
        _eventBus.OnHitEnemy -= HandleHitEnemy;
        _eventBus.OnPlayerDefend -= HandlePlayerDefend;
        _eventBus.OnPlayerHit -= HandleHitPlayer;
        _eventBus.OnEnemyDefend -= HandleEnemyDefend;

        // 아이템 구독 해제
        foreach (var item in ItemManager.instance.items)
            item.OnUnequip(_eventBus);
        foreach (var item in ItemManager.instance.artifacts)
            item.OnUnequip(_eventBus);

        // 보스 기믹 구독 해제
        if (enemyInfo is BossDataSo bossData)
            bossData.UnregisterAllGimmicks(_eventBus);
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

    public void ResetAllDiceVFX()
    {
        foreach (var dice in _attackDices) dice.VFX?.ResetBuff();
        foreach (var dice in _defenseDices) dice.VFX?.ResetBuff();
        foreach (var dice in _attackEnemyDices) dice.VFX?.ResetBuff();
        foreach (var dice in _defenseEnemyDices) dice.VFX?.ResetBuff();
    }

    public void ClearEnemyDices()
    {
        _attackEnemyDices.Clear();
        _defenseEnemyDices.Clear();
    }

    #endregion

    #region UI handler

    private void HandlePlayerHeal(DiceContext ctx, int amount)
    {
        battleUI.UpdatePlayerHP(playerData.CurrentHP, playerData.MaxHp);
    }

    private void HandleEnemyHeal(DiceContext ctx, int amount)
    {
        battleUI.UpdateEnemyHP(playerData.CurrentHP, playerData.MaxHp);
    }

    private void HandleHitPlayer(DiceContext ctx, int damage)
    {
        battleUI.UpdatePlayerShield(playerData.CurrentShield);
        battleUI.UpdatePlayerHP(playerData.CurrentHP, playerData.MaxHp);
        battleUI.ShowDamageText(damage, isPlayer: true);
    }

    private void HandleHitEnemy(DiceContext ctx, int damage)
    {
        battleUI.UpdateEnemyShield(enemyData.CurrentShield);
        battleUI.UpdateEnemyHP(enemyData.CurrentHP, enemyData.MaxHp);
        battleUI.ShowDamageText(damage, isPlayer: false);
    }

    private void HandleEnemyDefend(DiceContext ctx)
        => battleUI.UpdateEnemyShield(enemyData.CurrentShield);

    private void HandlePlayerDefend(DiceContext ctx)
        => battleUI.UpdatePlayerShield(playerData.CurrentShield);

    #endregion

    #region etc..

    public void SetDiceInfo(List<Dice> attackDices, List<Dice> defenseDices)
    {
        _attackDices = attackDices;
        _defenseDices = defenseDices;
    }

    public void SetEnemyAttackDice(Dice attackEnemyDice)
        => _attackEnemyDices.Add(attackEnemyDice);

    public void SetEnemyDefenseDice(Dice defenseEnemyDice)
        => _defenseEnemyDices.Add(defenseEnemyDice);

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

        playerData.Initialize(playerSO, data.playerCurrentHP);
        enemyData.Initialize(BattleDataManager.instance.currentEnemyData, data.enemyMaxHP);

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