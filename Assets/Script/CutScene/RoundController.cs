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
    [SerializeField] private RoundIntroController turnEffect;
    //[SerializeField] private BattlePanelAnimation battlePanel;

    [Header("게임 UI")]
    [SerializeField] private CanvasGroup playerUIGroup;
    [SerializeField] private CanvasGroup enemyUIGroup;
    [SerializeField] private CanvasGroup[] HideUIGroup;
    [SerializeField] private float fadeDuration = 0.4f;

    [SerializeField] private DiceSpawnAnimation diceSpawnAnimation;

    private CancellationTokenSource cts;

    void Awake()
    {
        cts = new CancellationTokenSource();
    }

    private void HideGameUI()
    {
        foreach (var h in HideUIGroup)
        {
            h.alpha = 0f;
        }
    }

    public void PlayIntroAnim(int currentRound = 1)
    {
        PlayRoundSequenceAsync(cts.Token,currentRound).Forget();
    }

    private async UniTask PlayRoundSequenceAsync(CancellationToken ct, int currentRound = 1)
    {
        roundIntroCanvas.gameObject.SetActive(true);
        HideGameUI();

        await PlayEffectAsync(onComplete => roundStartEffect.Play(onComplete), ct);
        await PlayEffectAsync(cardAppearEffect.Play, ct);
        //await PlayEffectAsync(roundCharacter.Play, ct);
        await PlayEffectAsync(playerEffect.Play, ct);
        await PlayEffectAsync(enemyEffect.Play, ct);
        // 여기서 기믹 추가

        await FadeInPlayerCharacter(ct);
        await FadeInEnemyCharacter(ct);
        
        playerUIGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);
        await enemyUIGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);

        BattleManager.instance.TriggerFirstTurnStart();

        await PlayEffectAsync(onComplete => turnEffect.Play(1, onComplete), ct);

        await UniTask.Delay(
           TimeSpan.FromSeconds(fadeDuration),
           cancellationToken: ct
        );

        await diceSpawnAnimation.PlayAsync(ct);

        await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: ct);

        await diceSpawnAnimation.PlayEnemyAsync(ct);

        await BeginRoundLogic(currentRound);
        roundIntroCanvas.gameObject.SetActive(false);      
    }

    public void NextTurn(int currentTurn = 1)
    {
        NextTurnUI(cts.Token, currentTurn).Forget();
    }

    private async UniTask NextTurnUI(CancellationToken ct, int currentTurn = 1)
    {
        roundIntroCanvas.gameObject.SetActive(true);
        await PlayEffectAsync(onComplete => turnEffect.Play(currentTurn, onComplete), ct);
        roundIntroCanvas.gameObject.SetActive(false);
    }

    private UniTask PlayEffectAsync(Action<Action> playFunc, CancellationToken ct)
    {
        var utcs = new UniTaskCompletionSource();
        playFunc(() => utcs.TrySetResult());
        return utcs.Task.AttachExternalCancellation(ct);
    }

    private async UniTask BeginRoundLogic(int currentRound)
    {
        await GameManager.instance.EnemyRoll();
    }

    private async UniTask FadeInEnemyCharacter(CancellationToken ct)
    {
        var enemyCharacter = BattleInitalizer.instance.EnemyCharacter;
        if (enemyCharacter != null)
            await enemyCharacter.FadeIn(0.5f).AttachExternalCancellation(ct);
    }

    private async UniTask FadeInPlayerCharacter(CancellationToken ct)
    {
        var playerCharacter = BattleInitalizer.instance.PlayerCharacter;
        if (playerCharacter != null)
            await playerCharacter.FadeIn(0.5f).AttachExternalCancellation(ct);
    }

    void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
