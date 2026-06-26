using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Linq;

public class NormalStageController : BaseStageController
{
    [Header("노말 스테이지 인트로 연출")]
    [SerializeField] private RoundIntro stageStartEffect;
    [SerializeField] private CardAppearEffect cardAppearEffect;
    [SerializeField] private RoundCharacter roundCharacter;
    [SerializeField] private CardAppearEffect playerEffect;
    [SerializeField] private CardAppearEffect enemyEffect;

    protected override async UniTask PlayIntroEffect(CancellationToken ct)
    {
        await PlayEffectAsync(onComplete => stageStartEffect.Play(1, onComplete), ct);
        await PlayEffectAsync(cardAppearEffect.Play, ct);
        await PlayEffectAsync(roundCharacter.Play, ct);
        await PlayEffectAsync(enemyEffect.Play, ct);
        await PlayEffectAsync(playerEffect.Play, ct);
    }

    protected override async UniTask FadeInEnemy(CancellationToken ct)
    {
        var enemyImage = BattleInitalizer.instance.enemyImage;
        if(enemyImage != null && enemyImage.gameObject.activeSelf)
        {
            enemyImage.color = new Color(1, 1, 1, 0);
            await enemyImage.DOFade(1f, fadeDuration)
                .SetEase(Ease.OutQuad)
                .ToUniTask()
                .AttachExternalCancellation(ct);
        }
    }

    protected override async UniTask FadeInGameUI(CancellationToken ct)
    {
        await UniTask.WhenAll(HideUIGroup.Select(group => FadeIn(group, ct)));
    }
}
