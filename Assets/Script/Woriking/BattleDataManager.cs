using UnityEngine;

public class BattleDataManager : MonoBehaviour
{
    public static BattleDataManager instance;

    [Header("전투 데이터")]
    public BaseEnemyData currentEnemyData;
    public bool isBossBattle;

    [Header("보상 데이터")]
    public RewardDataSo currentRewardData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void SetBattleData(EnemyData enemyData)
    {
        currentEnemyData = enemyData;
        isBossBattle = false;
        currentRewardData = enemyData.rewardData;
    }

    public void SetBossBattleData(BossDataSo bossData)
    {
        currentEnemyData = bossData;
        isBossBattle = true;
        currentRewardData = bossData.rewardData;
    }

    public void Clear()
    {
        currentEnemyData = null;
        currentRewardData = null;
        isBossBattle = false;
    }

    public int GetEnemyMaxHp()
    {
        return currentEnemyData?.maxHp ?? 0;
    }

    public GameObject GetEnemyPrefab()
    {
        return currentEnemyData.enemyPrefab;
    }

    public GameObject GetEnemyIntroPrefab()
    {
        return currentEnemyData.IntroPrefab;
    }

    public bool hasGimmick()
    {
        if (!isBossBattle) return false;
        return (currentEnemyData as BossDataSo)?.hasGimmick ?? false;
    }

    public int GetGoldReward()
    {
        return currentRewardData?.clearGold ?? 10;
    }
}

