using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BossDataSo", menuName = "Battle/BossDataSo")]
public class BossDataSo : BaseEnemyData
{
    [Header("«¡∏Æ∆’")]
    public GameObject enemyPrefab;

    [Header("±‚πÕ")]
    public bool hasGimmick;
    [SerializeField] private List<GimmickSo> gimmickList;

    [Header("¡÷ªÁ¿ß º≥¡§")]
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
