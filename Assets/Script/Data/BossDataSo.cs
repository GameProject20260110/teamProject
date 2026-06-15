using UnityEngine;

[CreateAssetMenu(fileName = "BossDataSo", menuName = "Battle/BossDataSo")]
public class BossDataSo : BaseEnemyData
{
    [Header("기믹")]
    public bool hasGimmick;

    [Header("주사위 설정")]
    public DiceData[] dicePool;
}
