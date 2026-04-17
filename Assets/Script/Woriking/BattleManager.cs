using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [Header("전투 데이터")]
    [SerializeField] private PlayerBattleData playerData;
    [SerializeField] private EnemyBattleData enemyData;

    [Header("reference")]
    [SerializeField] private BattleUI battleUI;

    public bool isPlayerTurn = true;
    public bool isBattleActive = false;
    public int currentTurn = 1;

    private CancellationTokenSource _battleCts;

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

        if (roundData != null)
        {
            int enemyHP = roundData.targetScore;
            enemyData.Initialize(enemyHP);

            if (PlayerManager.instance != null)
            {
                playerData.Initialize(PlayerManager.instance.heart);
            }
            else
            {
                playerData.Initialize();
            }

            battleUI.UpdateEnemyHP(enemyData.CurrentHP, enemyData.MaxHp);
            battleUI.UpdatePlayerHP(playerData.CurrentHP, playerData.MaxHp);

            isPlayerTurn = true;
            isBattleActive = true;
            currentTurn = 1;

            SaveBattleData();
        }
    }

    private int CalculateEnemyAttackPower()
    {
        return enemyData.CurrentHP;
    }

    public async UniTask OnPlayerAttack(int totalScore)
    {
        if (!isBattleActive || !isPlayerTurn) return;

        enemyData.TakeDamage(totalScore);
        battleUI.UpdateEnemyHP(enemyData.CurrentHP, enemyData.MaxHp);
        battleUI.ShowDamageText(totalScore, isPlayer: false);

        if (enemyData.IsDead())
        {
            OnBattleEnd();
            RoundManager.instance.CompleteRound(10000);
            return;
        }

        SaveBattleData();
        isPlayerTurn = false;

        try
        {
            await EnemyTurnRoutine(totalScore);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("전투가 취소되었습니다");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            isPlayerTurn = true;
        }

    }

    private async UniTask EnemyTurnRoutine(int playerFinalScore)
    {
        
        // 적 공격 연출 대기
        await UniTask.Delay(1000, cancellationToken: _battleCts.Token);

        int damage = CalculateEnemyAttackPower();
        playerData.TakeDamage(damage);

        battleUI.UpdatePlayerHP(playerData.CurrentHP, playerData.MaxHp);
        battleUI.ShowDamageText(damage, isPlayer: true);

        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.heart = playerData.CurrentHP;
            PlayerManager.instance.Save();
        }

        SaveBattleData();

        if (playerData.IsDead())
        {
            OnBattleEnd();
            RoundManager.instance.CompleteRound(0);
            return;
        }

        await UniTask.Delay(500, cancellationToken: _battleCts.Token);

        isPlayerTurn = true;
        currentTurn++;

        StartNewTurn();
        
    }

    private void StartNewTurn() // 턴 종료시 이벤트
    {
       battleUI.UpdateCurrentTurn(currentTurn);
    }

    private void OnBattleEnd()
    {
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

        // 데이터 복원
        playerData.Initialize(data.playerCurrentHP);
        enemyData.Initialize(data.enemyMaxHP);

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
