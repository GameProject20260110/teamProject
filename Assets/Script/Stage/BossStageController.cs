using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System.Linq;
using UnityEngine;

public class BossStageController : BaseStageController
{
    protected override async UniTask PlayIntroEffect(CancellationToken ct)
    {
        await UniTask.Delay(500, cancellationToken: ct);
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
            .ToUniTask())
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

        await seq.WithCancellation(ct);
    }
}
