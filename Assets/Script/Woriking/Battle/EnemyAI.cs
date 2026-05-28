using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI instance;
    void Awake() => instance = this;

    public void PlaceDice(Dice[] rolledDice)
    {
        var strategy = BattleDataManager.instance.currentEnemyData.aiStrategy;
        strategy.PlaceDice(rolledDice);
    }
}
