using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System.Linq;
using UnityEngine;
using System;

public class BossStageController : BaseStageController
{
    [SerializeField] private CharacterAppearEffect bossSpawnEffect;
    [SerializeField] private BossStageIntro bossStageIntro;
    protected override async UniTask PlayIntroEffect(CancellationToken ct)
    {
        await bossStageIntro.Play(ct);
    }

    protected override async UniTask FadeInEnemy(CancellationToken ct)
    {
        var bossData = BattleDataManager.instance.currentEnemyData as BossDataSo;
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
        if (BattleDataManager.instance.currentEnemyData is BossDataSo bossData)
            BossGimmickUIContainer.instance?.Setup(bossData.GimmickList);

        return UniTask.CompletedTask;
    }
}
