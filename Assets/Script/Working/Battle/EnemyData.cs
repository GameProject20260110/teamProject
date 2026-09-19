using UnityEngine;

[CreateAssetMenu(menuName = "Battle/EnemyData")]
public class EnemyData : BaseEnemyData
{
    [Header("주사위 설정")]
    public DiceData[] dicePool;
}
