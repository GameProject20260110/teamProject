using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

public abstract class BaseStageController : MonoBehaviour
{
    public static BaseStageController instance;

    [Header("공통")]
    [SerializeField] protected Canvas stageIntroCanvas;
    [SerializeField] protected RoundIntroController turnEffect;
    [SerializeField] protected float fadeDuration = 0.4f;
    [SerializeField] protected float interval = 0.17f;
    [SerializeField] protected DiceSpawnAnimation diceSpawnAnimation;

    [Header("게임UI")]
    [SerializeField] protected CanvasGroup[] HideUIGroup;

    protected CancellationTokenSource cts;

    private void Awake()
    {
        if (instance == null) instance = this;
        cts = new CancellationTokenSource();
    }

    protected void HideGameUI()
    {
        foreach(var h in HideUIGroup)
        {
            h.alpha = 0f;
            h.interactable = false;
            h.blocksRaycasts = false;
        }
    }

    protected void ShowGameUI()
    {
        foreach(var h in HideUIGroup)
        {
            h.interactable = true;
            h.blocksRaycasts = true;
        }
    }

    public void PlayIntroAnim()
    {
        PlayStageSequenceAsync(cts.Token).Forget();
    }

    protected async UniTask PlayStageSequenceAsync(CancellationToken ct)
    {
        try
        {
            stageIntroCanvas.gameObject.SetActive(true);
            HideGameUI();

            await PlayIntroEffect(ct);
            await FadeInGameUI(ct);
            await FadeInPlayer(ct);
            await FadeInEnemy(ct);
            ShowGameUI();

            if(diceSpawnAnimation != null)
            {
                await diceSpawnAnimation.PlayAsync(ct);
                await UniTask.Delay(200, cancellationToken: ct);
                await diceSpawnAnimation.PlayEnemyAsync(ct);
            }

            BattleManager.instance.TriggerFirstTurnStart();

            await PlayEffectAsync(onComplete => turnEffect.Play(1, onComplete), ct);
            await UniTask.Delay(200, cancellationToken: ct);
            await PlayBossGimmick(ct);
            await BeginStageLogic();
            stageIntroCanvas.gameObject.SetActive(false);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("종료");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    protected virtual UniTask PlayIntroEffect(CancellationToken ct) => UniTask.CompletedTask;
    protected virtual UniTask FadeInEnemy(CancellationToken ct) => UniTask.CompletedTask;
    protected virtual UniTask FadeInGameUI(CancellationToken ct) => UniTask.CompletedTask;
    protected virtual UniTask PlayBossGimmick(CancellationToken ct) => UniTask.CompletedTask;
    protected virtual UniTask FadeInPlayer(CancellationToken ct) => UniTask.CompletedTask;

    public void NextTurn(int currentTurn = 1)
    {
        NextTurnUI(cts.Token, currentTurn).Forget();
    }

    private async UniTask NextTurnUI(CancellationToken ct, int currentTurn = 1)
    {
        stageIntroCanvas.gameObject.SetActive(true);
        await PlayEffectAsync(onComplete => turnEffect.Play(currentTurn, onComplete), ct);
        stageIntroCanvas.gameObject.SetActive(false);
    }

    protected UniTask PlayEffectAsync(Action<Action> playFunc, CancellationToken ct)
    {
        var utcs = new UniTaskCompletionSource();
        playFunc(() => utcs.TrySetResult());
        return utcs.Task.AttachExternalCancellation(ct);
    }

    protected async UniTask BeginStageLogic()
    {
        await GameManager.instance.EnemyRoll();
    }

    protected async UniTask FadeIn(CanvasGroup cg, CancellationToken ct, float duration = -1f)
    {
        if (cg == null) return;
        float d = duration < 0 ? fadeDuration : duration;
        await cg.DOFade(1f, d)
            .SetEase(Ease.OutQuad)
            .ToUniTask(cancellationToken: ct);
    }

    protected virtual void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
