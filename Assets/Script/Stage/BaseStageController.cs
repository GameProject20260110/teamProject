using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

public abstract class BaseStageController : MonoBehaviour
{
    [Header("°øÅë")]
    [SerializeField] protected Canvas stageIntroCanvas;
    [SerializeField] protected RoundIntroController turnEffect;
    [SerializeField] protected float fadeDuration = 0.4f;
    [SerializeField] protected float interval = 0.17f;
    [SerializeField] protected DiceSpawnAnimation diceSpawnAnimation;

    [SerializeField] private SpriteGroupAlpha boardGroup;
    [SerializeField] private Camera mainCamera;

    private Physics2DRaycaster boardRaycaster;
    private LayerMask _fullEventMask;
    private int _boardLayerMask;

    [Header("¼û±è ´ë»ó UI")]
    [SerializeField] protected CanvasGroup[] HideUIGroup;

    protected CancellationTokenSource cts;

    private BattleManager _battleManager;
    private GameManager _gameManager;

    [Inject]
    public void Construct(BattleManager battleManager, GameManager gameManager)
    {
        _battleManager = battleManager;
        _gameManager = gameManager;
    }

    private void Awake()
    {
        cts = new CancellationTokenSource();
        boardRaycaster = mainCamera.GetComponent<Physics2DRaycaster>();
        _boardLayerMask = LayerMask.GetMask("Board");
        _fullEventMask = boardRaycaster.eventMask;
        boardRaycaster.eventMask &= ~_boardLayerMask;
    }

    protected async UniTask HideBoardAsync(CancellationToken ct, float duration = 0.4f)
    {
        boardRaycaster.eventMask &= ~_boardLayerMask;
        await boardGroup.FadeAsync(0f, duration, ct);
    }

    protected async UniTask ShowBoardAsync(CancellationToken ct, float duration = 0.4f)
    {
        await boardGroup.FadeAsync(1f, duration, ct);
        boardRaycaster.eventMask = _fullEventMask;
    }

    protected void HideGameUI()
    {
        foreach (var h in HideUIGroup)
        {
            h.alpha = 0f;
            h.interactable = false;
            h.blocksRaycasts = false;
        }
    }

    protected void ShowGameUI()
    {
        foreach (var h in HideUIGroup)
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
            await ShowBoardAsync(ct);     
            await FadeInPlayer(ct);
            await FadeInEnemy(ct);
            await FadeInGameUI(ct);
            ShowGameUI();
            if (diceSpawnAnimation != null)
            {
                await diceSpawnAnimation.PlayAsync(ct);
                await UniTask.Delay(200, cancellationToken: ct);
                await diceSpawnAnimation.PlayEnemyAsync(ct);
            }
            _battleManager.TriggerFirstTurnStart();
            await PlayEffectAsync(onComplete => turnEffect.Play(1, onComplete), ct);
            await UniTask.Delay(200, cancellationToken: ct);
            await PlayBossGimmick(ct);
            await BeginStageLogic();
            stageIntroCanvas.gameObject.SetActive(false);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Á¾·á");
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

    public UniTask NextTurn(int currentTurn = 1)
    {
        return NextTurnUI(cts.Token, currentTurn);
    }

    private async UniTask NextTurnUI(CancellationToken ct, int currentTurn = 1)
    {
        await diceSpawnAnimation.PlayAsync(ct);
        await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: ct);
        await diceSpawnAnimation.PlayEnemyAsync(ct);
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
        await _gameManager.EnemyRoll();
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
