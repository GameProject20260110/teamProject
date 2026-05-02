using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using DG.Tweening;

public class RoundController : MonoBehaviour
{
    [SerializeField] private Canvas roundIntroCanvas;
    [SerializeField] private RoundIntro roundStartEffect;
    [SerializeField] private CardAppearEffect cardAppearEffect;
    [SerializeField] private RoundCharacter roundCharacter;
    [SerializeField] private CardAppearEffect playerEffect;
    [SerializeField] private CardAppearEffect enemyEffect;

    [Header("°ÔÀÓ UI")]
    [SerializeField] private CanvasGroup playerUIGroup;
    [SerializeField] private CanvasGroup enemyUIGroup;
    [SerializeField] private float fadeDuration = 0.4f;

    private CancellationTokenSource cts;

    void Start()
    {
        cts = new CancellationTokenSource();
        PlayRoundSequenceAsync(cts.Token).Forget();
    }

    private void HideGameUI()
    {
        playerUIGroup.alpha = 0f;
        enemyUIGroup.alpha = 0f;
    }

    public async UniTask PlayRoundSequenceAsync(CancellationToken ct, int currentRound = 1)
    {
        roundIntroCanvas.gameObject.SetActive(true);
        HideGameUI();

        await PlayEffectAsync(onComplete => roundStartEffect.Play(currentRound, onComplete), ct);
        await PlayEffectAsync(cardAppearEffect.Play, ct);
        await PlayEffectAsync(roundCharacter.Play, ct);
        await PlayEffectAsync(playerEffect.Play, ct);
        await PlayEffectAsync(enemyEffect.Play, ct);

        roundIntroCanvas.gameObject.SetActive(false);
        await ShowGameUIAsync(ct);

        BeginRoundLogic(currentRound);
    }

    private UniTask PlayEffectAsync(Action<Action> playFunc, CancellationToken ct)
    {
        var utcs = new UniTaskCompletionSource();
        playFunc(() => utcs.TrySetResult());
        return utcs.Task.AttachExternalCancellation(ct);
    }

    private async UniTask ShowGameUIAsync(CancellationToken ct)
    {
        playerUIGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);
        enemyUIGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);

        await UniTask.Delay(
            TimeSpan.FromSeconds(fadeDuration),
            cancellationToken: ct
        );
    }

    private void BeginRoundLogic(int currentRound)
    {
    }

    void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
