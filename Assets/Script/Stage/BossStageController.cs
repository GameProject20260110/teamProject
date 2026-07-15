using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System.Linq;
using UnityEngine;
using VContainer;

public class BossStageController : BaseStageController
{
    [SerializeField] private CharacterAppearEffect bossSpawnEffect;
    [SerializeField] private CardAppearEffect cardAppearEffect;
    [SerializeField] private BossStageIntro bossStageIntro;

    private BattleDataManager _battleDataManager;

    [Inject]
    public void Construct(BattleDataManager battleDataManager, AudioManager audioManager, BattleInitalizer battleInitalizer)
    {
        _battleDataManager = battleDataManager;
        bossSpawnEffect.SetDependencies(audioManager, battleInitalizer);
    }

    protected override async UniTask PlayIntroEffect(CancellationToken ct)
    {
        await bossStageIntro.Play(ct);
    }

    protected override async UniTask FadeInPlayer(CancellationToken ct)
    {
        await PlayEffectAsync(cardAppearEffect.Play,ct);
    }

    protected override async UniTask FadeInEnemy(CancellationToken ct)
    {
        var bossData = _battleDataManager.currentEnemyData as BossDataSo;
        if (bossData == null || bossData.enemyPrefab == null) return;

        bossSpawnEffect.SetPrefab(bossData.enemyPrefab);
        bossSpawnEffect.SetSpawnOverride(bossData.spawnPosition, bossData.bossScale);
        await PlayEffectAsync(OnComplete => bossSpawnEffect.Play(OnComplete), ct);
    }

    protected override async UniTask FadeInGameUI(CancellationToken ct)
    {
        Sequence seq = DOTween.Sequence();

        foreach(var group in HideUIGroup)
        {
            seq.Append(group.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad));
            seq.AppendInterval(interval);
        }

        seq.SetLink(gameObject);
        await seq.WithCancellation(ct);
    }

    protected override UniTask PlayBossGimmick(CancellationToken ct)
    {
        if (_battleDataManager.currentEnemyData is BossDataSo bossData)
            BossGimmickUIContainer.instance?.Setup(bossData.GimmickList);

        return UniTask.CompletedTask;
    }
}
