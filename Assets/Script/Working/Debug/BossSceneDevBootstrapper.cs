using UnityEngine;
using VContainer;
using VContainer.Unity;

public class BossSceneDevBootstrapper : MonoBehaviour, IInitializable
{
#if UNITY_EDITOR
    [Header("Boss 씬 단독 테스트용 (Map 안 거치고 Play 할 때만 사용)")]
    [SerializeField] private BossDataSo debugBossData;
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
        if (_battleDataManager.currentEnemyData == null && debugBossData != null)
        {
            _battleDataManager.SetBossBattleData(debugBossData);
            Debug.Log("[DevBootstrapper] Map을 안 거쳐서 더미 보스 데이터로 채웠습니다.");
        }
#endif
    }
}