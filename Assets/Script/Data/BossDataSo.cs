using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BossDataSo", menuName = "Battle/BossDataSo")]
public class BossDataSo : BaseEnemyData
{
    [Header("프리팹")]
    public GameObject enemyPrefab;

    [Header("보스 설정")]
    public Vector3 bossScale = new Vector3(0.42f, 0.42f, 1f);
    public Vector3 spawnPosition = new Vector3(0, 1.3f, 0);

    [Header("기믹")]
    public bool hasGimmick;
    [SerializeField] private List<GimmickSo> gimmickList;

    [Header("주사위 설정")]
    public DiceData[] dicePool;

//<<<<<<< Updated upstream
//    [Header("정렬")]
//    public int sortingOrder = 0;
//=======
    [Header("대사")]
    public string[] appearDialogues;
    public string battleDialogue;

    public List<GimmickSo> GimmickList => gimmickList;
//>>>>>>> Stashed changes

    public void RegisterAllGimmicks(BattleEventBus bus)
    {
        //BossGimmickUIContainer.instance.Setup(gimmickList);

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
