using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System.Linq;
using UnityEngine;

public class BossStageController : BaseStageController
{
    [SerializeField] private BossStageIntro bossStageIntro;
    protected override async UniTask PlayIntroEffect(CancellationToken ct)
    {
        await bossStageIntro.Play(ct);
    }

    protected override async UniTask FadeInEnemy(CancellationToken ct)
    {
        var enemy = BattleInitalizer.instance.spawnEnemy;
        if (enemy == null) return;

        var renderers = enemy.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
            sr.color = new Color(1, 1, 1, 0);

        await UniTask.WhenAll(renderers.Select(sr => sr.DOFade(1f, 0.5f)
            .SetEase(Ease.OutQuad)
            .ToUniTask(TweenCancelBehaviour.Kill, ct))
        ).AttachExternalCancellation(ct);
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
        if (BattleDataManager.instance.currentEnemyData is BossDataSo bossData)
            BossGimmickUIContainer.instance?.Setup(bossData.GimmickList);

        return UniTask.CompletedTask;
    }
}
