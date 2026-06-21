using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BossDataSo", menuName = "Battle/BossDataSo")]
public class BossDataSo : BaseEnemyData
{
    [Header("기믹")]
    public bool hasGimmick;
    [SerializeField] private List<GimmickSo> gimmickList;

    [Header("주사위 설정")]
    public DiceData[] dicePool;

    public void RegisterAllGimmicks(BattleEventBus bus)
    {
        BossGimmickUIContainer.instance.Setup(gimmickList);

        foreach (var gimm in gimmickList)
        {
            gimm.Register(bus);
        }
    }

    public void UnregisterAllGimmicks(BattleEventBus bus)
    {
        foreach (var gimm in gimmickList)
        {
            gimm.Unregister(bus);
        }

        BossGimmickUIContainer.instance.Clear();
    }
}
