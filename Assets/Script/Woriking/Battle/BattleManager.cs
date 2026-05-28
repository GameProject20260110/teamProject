using Cysharp.Threading.Tasks;
using System;
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

    [Header("reference")]
    [SerializeField] private BattleUI battleUI;
    [SerializeField] private EnemyDeathSequence enemyDeathSequence;

    public bool isPlayerTurn = true;
    public bool isBattleActive = false;
    public int currentTurn = 1;
    public GameObject EnemySkillPrefab;

    private CancellationTokenSource _battleCts;
    private int enemyDamage;

    private List<Dice> _attackDices = new List<Dice>();
    private List<Dice> _defenseDices = new List<Dice>();
    private List<Dice> _attackEnemyDices = new List<Dice>();
    private List<Dice> _defenseEnemyDices = new List<Dice>();

    private BattleEventBus _eventBus;
    private BattleContextFactory _ctxFactory;
    private BattleContextFactory _enemyctxFactory;
    private BattleSaveHandler _saveHandler;

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

    public void InitializeBattle()
    {
        if (BattleInitalizer.instance == null) return;

        _battleCts?.Cancel();
        _battleCts?.Dispose();
        _battleCts = new CancellationTokenSource();

        enemyData.Initialize(BattleDataManager.instance.currentEnemyData);
        playerData.Initialize(playerSO);

        isPlayerTurn = false;
        isBattleActive = true;
        currentTurn = 1;

        // 새 버스 생성 — 이전 구독 자동 정리
        _eventBus = new BattleEventBus();

        // BattleManager UI 구독
        _eventBus.OnHitEnemy += HandleHitEnemy;
        _eventBus.OnPlayerDefend += HandlePlayerDefend;
        _eventBus.OnPlayerHit += HandleHitPlayer;
        _eventBus.OnEnemyDefend += HandleEnemyDefend;

        // 아이템 구독
        foreach (var item in ItemManager.instance.items)
            item.OnEquip(_eventBus);
        foreach (var item in ItemManager.instance.Artifacts)
            item.OnEquip(_eventBus);

        _ctxFactory = new BattleContextFactory(
            playerData, enemyData,
            Enemytrans, Playertrans,
            _attackDices, _defenseDices,
            _eventBus,
            _battleCts.Token,
            isPlayer : true
        );

        _enemyctxFactory = new BattleContextFactory(
            playerData, enemyData,
            Enemytrans, Playertrans,
            _attackDices, _defenseDices,
            _eventBus,
            _battleCts.Token,
            isPlayer: false
        );

        battleUI.UpdateEnemyHP(enemyData.CurrentHP, enemyData.MaxHp);
        battleUI.UpdatePlayerHP(playerData.CurrentHP, playerData.MaxHp);

        _saveHandler.Save(isPlayerTurn, isBattleActive, currentTurn);
    }

    private void HandleHitPlayer(BattleContext ctx, int damage)
    {
        battleUI.UpdatePlayerShield(playerData.CurrentShield);
        battleUI.UpdatePlayerHP(playerData.CurrentHP, playerData.MaxHp);
        battleUI.ShowDamageText(damage, isPlayer: false);
    }

    private void HandleHitEnemy(BattleContext ctx, int damage)
    {
        battleUI.UpdateEnemyShield(enemyData.CurrentShield);
        battleUI.UpdateEnemyHP(enemyData.CurrentHP, enemyData.MaxHp);
        battleUI.ShowDamageText(damage, isPlayer: true);
    }

    private void HandleEnemyDefend(BattleContext ctx)
    {
        battleUI.UpdateEnemyShield(enemyData.CurrentShield);
    }

    private void HandlePlayerDefend(BattleContext ctx)
    {
        battleUI.UpdatePlayerShield(playerData.CurrentShield);
    }

    public void SetDiceInfo(List<Dice> attackDices, List<Dice> defenseDices)
    {
        _attackDices = attackDices;
        _defenseDices = defenseDices;      
    }

    public void SetEnemyAttackDice(Dice attackEnemyDices)
    {
        _attackEnemyDices.Add(attackEnemyDices);
    }

    public void SetEnemyDefenseDice(Dice defenseEnemyDices)
    {
        _defenseEnemyDices.Add(defenseEnemyDices);
    }

    private int CalculateEnemyAttackPower() => UnityEngine.Random.Range(7, 15);

    public async UniTask EnemyDefense()
    {
        //if (!isBattleActive || isPlayerTurn) return;

        foreach (var dice in _defenseEnemyDices)
        {
            Debug.Log(123);

            if (dice == null) continue;
            await dice.Glow.ShowGlowAsync();
            var ctx = _enemyctxFactory.Create(dice.MyState.originalValue, dice.MyState.diceData);
            await dice.Effect.OnDefense(ctx);
            dice.Glow.HideGlow();
        }

        isPlayerTurn = true;
        try
        {
            await UniTask.Delay(500);
            await OnPlayerAttack();
        }
        catch (OperationCanceledException oce)
        {
            Debug.Log($"전투가 취소되었습니다 {oce.Message}\n{oce.StackTrace}");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            isPlayerTurn = false;
        }
    }

    private async UniTask OnPlayerAttack()
    {
        foreach (var dice in _attackDices)
        {
            await dice.Glow.ShowGlowAsync();
            var ctx = _ctxFactory.Create(dice.MyState.originalValue, dice.MyState.diceData);
            await dice.Effect.OnAttack(ctx);
            dice.Glow.HideGlow();
        }

        if (enemyData.IsDead())
        {
            _eventBus.TriggerEnemyDead(_ctxFactory.Create());
            await enemyDeathSequence.PlayDeathSequence(Enemytrans.position);
            OnBattleEnd();
            BattleInitalizer.instance.CompleteBattle(true);
            return;
        }

        try
        {
            await UniTask.Delay(500);
            await PlayerDefense();
        }
        catch (OperationCanceledException oce)
        {
            Debug.Log($"전투가 취소되었습니다 {oce.Message}\n{oce.StackTrace}");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            isPlayerTurn = true;
        }
    }

    private async UniTask PlayerDefense()
    {
        foreach (var dice in _defenseDices)
        {
            await dice.Glow.ShowGlowAsync();
            var ctx = _ctxFactory.Create(dice.MyState.originalValue, dice.MyState.diceData);
            await dice.Effect.OnDefense(ctx);
            dice.Glow.HideGlow();
        }

        _saveHandler.Save(isPlayerTurn, isBattleActive, currentTurn);

        isPlayerTurn = false;

        try
        {
            await UniTask.Delay(500);
            await EnemyAttack();
        }
        catch (OperationCanceledException oce)
        {
            Debug.Log($"전투가 취소되었습니다 {oce.Message}\n{oce.StackTrace}");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            isPlayerTurn = true;
        }
    }

    private async UniTask EnemyAttack()
    {
        foreach (var dice in _attackEnemyDices)
        {
            if (dice == null) continue;
            await dice.Glow.ShowGlowAsync();
            var ctx = _enemyctxFactory.Create(dice.MyState.originalValue, dice.MyState.diceData);
            await dice.Effect.OnAttack(ctx);
            dice.Glow.HideGlow();
        }

        if (playerData.IsDead())
        {
            OnBattleEnd();
            BattleInitalizer.instance.CompleteBattle(false);
            return;
        }

        isPlayerTurn = true;

        try
        {
            await UniTask.Delay(500);
            await StartNewTurn();
        }
        catch (OperationCanceledException oce)
        {
            Debug.Log($"전투가 취소되었습니다 {oce.Message}\n{oce.StackTrace}");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            isPlayerTurn = false;
        }
    }

    

    //private async UniTask EnemyTurnRoutine()
    //{
    //    await UniTask.Delay(500, cancellationToken: _battleCts.Token);

    //    int damage = enemyDamage;
    //    var attackCompletion = new UniTaskCompletionSource<bool>();

    //    GameObject skill = ObjectPool.instance.Get(EnemySkillPrefab);
    //    skill.transform.position = Playertrans.position;

    //    skill.GetComponent<Skill>().Init(new SkillContext
    //    {
    //        isPlayer = false,
    //        damage = damage,
    //        onHit = () =>
    //        {
    //            int actualDamage = playerData.TakeDamage(damage);
    //            battleUI.UpdatePlayerShield(playerData.CurrentShield);
    //            battleUI.UpdatePlayerHP(playerData.CurrentHP, playerData.MaxHp);
    //            battleUI.ShowDamageText(actualDamage, isPlayer: true);
    //            _eventBus.TriggerPlayerHit(_ctxFactory.Create());

    //            if (PlayerManager.instance != null)
    //            {
    //                PlayerManager.instance.heart = playerData.CurrentHP;
    //                PlayerManager.instance.Save();
    //            }
    //            _saveHandler.Save(isPlayerTurn, isBattleActive, currentTurn);
    //        },
    //        onEnd = () => attackCompletion?.TrySetResult(true),
    //        startPos = Playertrans.position,
    //        targetPos = Enemytrans.position
    //    });

    //    await attackCompletion.Task.AttachExternalCancellation(_battleCts.Token);

    //    if (playerData.IsDead())
    //    {
    //        OnBattleEnd();
    //        BattleInitalizer.instance.CompleteBattle(false);
    //        //RoundManager.instance.CompleteRound(false);
    //        return;
    //    }

    //    await UniTask.Delay(500, cancellationToken: _battleCts.Token);
    //    isPlayerTurn = true;
    //    currentTurn++;
    //    await StartNewTurn();
    //}

    private async UniTask StartNewTurn()
    {
        var ctx = _ctxFactory.Create();
        var enemyCtx = _enemyctxFactory.Create();
        await enemyData.ProcessTurnStart(enemyCtx);
        await playerData.ProcessTurnStart(ctx);

        if (enemyData.IsDead())
        {
            _eventBus.TriggerEnemyDead(ctx);
            await enemyDeathSequence.PlayDeathSequence(Enemytrans.position);
            OnBattleEnd();
            //RoundManager.instance.CompleteRound(true);
            BattleInitalizer.instance.CompleteBattle(true);
            return;
        }

        enemyDamage = CalculateEnemyAttackPower();
        playerData.ResetShield();
        enemyData.ResetShield();

        battleUI.UpdatePlayerShield(playerData.CurrentShield);
        battleUI.UpdateEnemyShield(enemyData.CurrentShield);

        DeckManager.instance.DrawDice();
        EnemyDeckManager.instance.DrawEnemyDice();

        foreach (var dice in _attackDices)
            dice.VFX?.ResetBuff();
        foreach (var dice in _defenseDices)
            dice.VFX?.ResetBuff();
        foreach (var dice in _attackEnemyDices)
            dice.VFX?.ResetBuff();
        foreach (var dice in _defenseEnemyDices)
            dice.VFX?.ResetBuff();

        _eventBus.TriggerTurnStart(ctx);

        UiController.instance.ShowGlowRerollBtn();
        battleUI.UpdateCurrentTurn(currentTurn);
    }

    private void OnBattleEnd()
    {
        currentTurn = 1;
        playerData.ResetShield();
        enemyData.ResetShield();
        battleUI.UpdatePlayerShield(playerData.CurrentShield);
        battleUI.UpdateEnemyShield(enemyData.CurrentShield);
        isBattleActive = false;
        _battleCts?.Cancel();
        _saveHandler.Delete();

        // BattleManager 구독 해제
        _eventBus.OnHitEnemy -= HandleHitEnemy;
        _eventBus.OnPlayerDefend -= HandlePlayerDefend;
        _eventBus.OnPlayerHit -= HandleHitPlayer;
        _eventBus.OnEnemyDefend -= HandleEnemyDefend;

        // 아이템 구독 해제
        foreach (var item in ItemManager.instance.items)
            item.OnUnequip(_eventBus);
        foreach (var item in ItemManager.instance.Artifacts)
            item.OnUnequip(_eventBus);
    }

    public void UseItem(BattleItemSo item)
    {
        if (!isBattleActive) return;

        var ctx = _ctxFactory.Create();
        item.OnUse(ctx);

        if (item.isConsumable)
        {
            ItemManager.instance.items.Remove(item);
            UiController.instance.RefreshInventory();
        }
    }

    public void SaveBattleData() => _saveHandler.Save(isPlayerTurn, isBattleActive, currentTurn);

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
}