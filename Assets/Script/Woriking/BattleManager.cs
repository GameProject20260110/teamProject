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

    private const string BATTLE_SAVE_FILE = "battleData.json";

    private void OnDestroy()
    {
        _battleCts?.Cancel();
        _battleCts?.Dispose();
    }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadBattleData();
    }

    public void InitializeBattle()
    {
        if (RoundManager.instance == null || RoundManager.instance.currentStageData == null)
            return;

        _battleCts?.Cancel();
        _battleCts?.Dispose();
        _battleCts = new CancellationTokenSource();

        RoundData roundData = RoundManager.instance.currentStageData.GetRoundData(
            RoundManager.instance.currentRound
        );

        if (roundData != null && roundData.enemyData != null)
        {
            enemyData.Initialize(roundData.enemyData);

            playerData.Initialize(playerSO);

            battleUI.UpdateEnemyHP(enemyData.CurrentHP, enemyData.MaxHp);
            battleUI.UpdatePlayerHP(playerData.CurrentHP, playerData.MaxHp);

            isPlayerTurn = true;
            isBattleActive = true;
            currentTurn = 1;

            SaveBattleData();
        }

        enemyDamage = CalculateEnemyAttackPower();
        battleUI.UpdateEnemyAttackAmount(enemyDamage);
    }

    //public void SetPlayerStats(int attackPower, int defensePower)
    //{
    //    playerData.SetPlayerStats(attackPower, defensePower);
    //    battleUI.UpdatePlayerShield(playerData.CurrentShield);
    //}

    public void SetDiceInfo(List<Dice> attackDices, List<Dice> defenseDices)
    {
        _attackDices = attackDices;
        _defenseDices = defenseDices;
    }

    private int CalculateEnemyAttackPower()
    {
        return UnityEngine.Random.Range(7, 15);
    }

    public async UniTask OnPlayerAttack()
    {
        if (!isBattleActive || !isPlayerTurn) return;

        foreach(var dice in _attackDices)
        {
            await dice.GetComponentInChildren<DiceGlow>().ShowGlowAsync();

            var ctx = new BattleContext
            {
                Player = playerData,
                Enemy = enemyData,
                EnemyPosition = Enemytrans.position,
                BaseDamage = dice.MyState.originalValue,
                CancellationToken = _battleCts.Token,
                diceData = dice.MyState.diceData,
                OnEnemyHit = (damage) =>
                {
                    battleUI.UpdateEnemyHP(enemyData.CurrentHP, enemyData.MaxHp);
                    battleUI.ShowDamageText(damage, isPlayer: false);
                }
                
            };

            await dice.GetComponent<DiceEffectBase>().OnAttack(ctx);

            dice.GetComponentInChildren<DiceGlow>().HideGlow();
        }

        if (enemyData.IsDead())
        {
            await enemyDeathSequence.PlayDeathSequence(Enemytrans.position);
            OnBattleEnd();
            RoundManager.instance.CompleteRound(true);
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
        foreach(var dice in _defenseDices)
        {
            await dice.GetComponentInChildren<DiceGlow>().ShowGlowAsync();

            var ctx = new BattleContext
            {
                Player = playerData,
                Enemy = enemyData,
                PlayerPosition = Playertrans.position,
                BaseDamage = dice.MyState.originalValue,
                CancellationToken = _battleCts.Token,
                diceData = dice.MyState.diceData,
                OnPlayerDefend = (shield) =>
                {
                    battleUI.UpdatePlayerShield(playerData.CurrentShield);
                }
            };

            await dice.GetComponent<DiceEffectBase>().OnDefense(ctx);

            dice.GetComponentInChildren<DiceGlow>().HideGlow();
        }       

        SaveBattleData();
        isPlayerTurn = false;

        try
        {
            await UniTask.Delay(500);
            await EnemyTurnRoutine();
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

    private async UniTask EnemyTurnRoutine()
    {
        await UniTask.Delay(500, cancellationToken: _battleCts.Token);


        int damage = enemyDamage;
        var attackCompletion = new UniTaskCompletionSource<bool>();

        GameObject skill = ObjectPool.instance.Get(EnemySkillPrefab); // enum에 추가 필요
        skill.transform.position = Playertrans.position; // 플레이어 위치로

        skill.GetComponent<Skill>().Init(new SkillContext {
            isPlayer = false,
            damage = damage,
            onHit = () =>
            {
                int actualDamage = playerData.TakeDamage(damage);
                battleUI.UpdatePlayerShield(playerData.CurrentShield);
                battleUI.UpdatePlayerHP(playerData.CurrentHP, playerData.MaxHp);
                battleUI.ShowDamageText(actualDamage, isPlayer: true);

                // 플레이어 데이터 저장
                if (PlayerManager.instance != null)
                {
                    PlayerManager.instance.heart = playerData.CurrentHP;
                    PlayerManager.instance.Save();
                }
                SaveBattleData();
            },
            onEnd = () =>
            {
                attackCompletion?.TrySetResult(true);
            },
            startPos = Playertrans.position,
            targetPos = Enemytrans.position
        });

        await attackCompletion.Task.AttachExternalCancellation(_battleCts.Token);

        if (playerData.IsDead())
        {
            OnBattleEnd();
            RoundManager.instance.CompleteRound(false);
            return;
        }

        await UniTask.Delay(500, cancellationToken: _battleCts.Token);
        isPlayerTurn = true;
        currentTurn++;
        await StartNewTurn();
    }

    private async UniTask StartNewTurn() // 턴 종료시 이벤트
    {
        
        //playerData.ProcessTurnStart(Playertrans.position);

        var ctx = new BattleContext
        {
            Player = playerData,
            Enemy = enemyData,
            EnemyPosition = Enemytrans.position,
            CancellationToken = _battleCts.Token,
            OnEnemyHit = (damage) =>
            {
                battleUI.UpdateEnemyHP(enemyData.CurrentHP, enemyData.MaxHp);
                battleUI.ShowDamageText(damage, isPlayer: false);
            }
        };

        await enemyData.ProcessTurnStart(ctx);

        if (enemyData.IsDead())
        {
            await enemyDeathSequence.PlayDeathSequence(Enemytrans.position);
            OnBattleEnd();
            RoundManager.instance.CompleteRound(true);
            return;
        }

        enemyDamage = CalculateEnemyAttackPower();
        playerData.ResetShield();

        battleUI.UpdatePlayerShield(playerData.CurrentShield);
        battleUI.UpdateEnemyAttackAmount(enemyDamage);

        DeckManager.instance.DrawDice();

        UiController.instance.ShowGlowRerollBtn();
        battleUI.UpdateCurrentTurn(currentTurn);
    }

    private void OnBattleEnd()
    {
        currentTurn = 1;
        playerData.ResetShield();
        battleUI.UpdatePlayerShield(playerData.CurrentShield);
        isBattleActive = false;
        _battleCts?.Cancel();
        DeleteBattleData();

        Debug.Log($"{currentTurn} 턴)");
    }



    public void SaveBattleData()
    {
        if (SaveManager.instance == null) return;

        BattleSaveData data = new BattleSaveData(
            playerData,
            enemyData,
            isPlayerTurn,
            isBattleActive
        );

        data.currentBattleRound = this.currentTurn;

        SaveManager.instance.Save(data, BATTLE_SAVE_FILE);
        Debug.Log("전투 데이터 저장");
    }

    public void LoadBattleData()
    {
        if (SaveManager.instance == null) return;

        if (!SaveManager.instance.HasSaveFile(BATTLE_SAVE_FILE))
        {
            Debug.Log("저장된 전투 데이터 없음");
            return;
        }

        BattleSaveData data = SaveManager.instance.Load<BattleSaveData>(BATTLE_SAVE_FILE);

        RoundData roundData = RoundManager.instance.currentStageData.GetRoundData(data.currentBattleRound);

        // 데이터 복원
        playerData.Initialize(playerSO, data.playerCurrentHP);
        enemyData.Initialize(roundData.enemyData, data.enemyMaxHP);

        isPlayerTurn = data.isPlayerTurn;
        isBattleActive = data.isBattleActive;
        currentTurn = data.currentBattleRound;

        // UI 업데이트
        battleUI.UpdatePlayerHP(playerData.CurrentHP, playerData.MaxHp);
        battleUI.UpdateEnemyHP(enemyData.CurrentHP, enemyData.MaxHp);

        Debug.Log("전투 데이터 로드 완료");
    }

    public void DeleteBattleData()
    {
        if (SaveManager.instance != null)
        {
            SaveManager.instance.Delete(BATTLE_SAVE_FILE);
            Debug.Log("전투 데이터 삭제");
        }
    }
}
