using UnityEngine;

public class EnemyDeckHandler : MonoBehaviour
{
    public static EnemyDeckHandler instance;
    void Awake() => instance = this;

    public void SetupEnemyDice()
    {
        var baseData = BattleDataManager.instance.currentEnemyData;
        DiceManager.instance.ClearEnemyAllSlots();

        // 일반 몹
        if (baseData is EnemyData enemyData)
        {
            for (int i = 0; i < enemyData.dicePool.Length; i++)
                DiceManager.instance.EnemyPlaceDice(i, enemyData.dicePool[i]);
        }
        // 보스
        else if (baseData is BossDataSo bossData)
        {
            for (int i = 0; i < bossData.dicePool.Length; i++)
                DiceManager.instance.EnemyPlaceDice(i, bossData.dicePool[i]);
        }
    }
}
