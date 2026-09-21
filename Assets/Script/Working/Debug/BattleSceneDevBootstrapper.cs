using UnityEngine;
using VContainer;
using VContainer.Unity;

public class BattleSceneDevBootstrapper : MonoBehaviour, IInitializable
{
#if UNITY_EDITOR
    [Header("전투 씬 단독 테스트용 (Map 안 거치고 Play 할 때만 사용)")]
    [SerializeField] private EnemyData debugEnemyData;
#endif

    private BattleDataManager _battleDataManager;

    [Inject]
    public void Construct(BattleDataManager battleDataManager)
    {
        _battleDataManager = battleDataManager;
    }

    public void Initialize()
    {
#if UNITY_EDITOR
        if ((_battleDataManager.currentEnemyData == null || _battleDataManager.isBossBattle) && debugEnemyData != null)
        {
            _battleDataManager.SetBattleData(debugEnemyData);
            Debug.Log("[DevBootstrapper] Map을 안 거쳐서 더미 일반 몹 데이터로 채웠습니다.");
        }
#endif
    }
}
