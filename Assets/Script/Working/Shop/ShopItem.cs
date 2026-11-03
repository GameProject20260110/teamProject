using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public abstract class ShopItem<T> : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    where T : class
{
    [Header("References")]
    [SerializeField] protected TextMeshProUGUI goldText;
    [SerializeField] private Image holdProgressFill;

    [Header("Settings")]
    [SerializeField] private float holdDuration = 0.5f;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float flyEndScale = 0.2f;

    public T Data { get; protected set; }

    protected bool isSold;
    private Vector3 originScale;
    private CanvasGroup canvasGroup;
    private CancellationTokenSource _holdCts;

    public RectTransform inventoryIconRect { protected get; set; }
    public Transform ShopCanvas { protected get; set; }

    private Func<T, bool> _onBuy;
    private Action<T> _onHoverEnter;
    private Action<T> _onHoverExit;

    protected virtual void Awake()
    {
        originScale = transform.localScale;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Bind(Func<T, bool> onBuy, Action<T> onHoverEnter = null, Action<T> onHoverExit = null)
    {
        _onBuy = onBuy;
        _onHoverEnter = onHoverEnter;
        _onHoverExit = onHoverExit;
    }

    protected abstract void ApplyData(T data);

    public void Setup(T data)
    {
        isSold = false;
        gameObject.SetActive(true);
        transform.DOKill();
        transform.localScale = originScale;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        ApplyData(data);
    }

    public void Hide() => gameObject.SetActive(false);

    private void OnDisable() => CancelHold();

    private void OnDestroy()
    {
        CancelHold();
        transform.DOKill();
    }

    #region Pointer Events

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSold) return;
        ScaleTo(originScale * hoverScale);
        _onHoverEnter?.Invoke(Data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSold) return;
        ScaleTo(originScale);
        _onHoverExit?.Invoke(Data);
        CancelHold();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isSold || eventData.button != PointerEventData.InputButton.Left) return;
        CancelHold();
        _holdCts = new CancellationTokenSource();
        HoldToBuyAsync(_holdCts.Token).Forget();
    }

    public void OnPointerUp(PointerEventData eventData) => CancelHold();

    #endregion

    #region Hold To Buy

    private void CancelHold()
    {
        _holdCts?.Cancel();
        _holdCts?.Dispose();
        _holdCts = null;
        if (holdProgressFill != null)
        {
            holdProgressFill.fillAmount = 0f;
            holdProgressFill.gameObject.SetActive(false);
        }
    }

    private async UniTaskVoid HoldToBuyAsync(CancellationToken ct)
    {
        if (holdProgressFill != null)
        {
            holdProgressFill.fillAmount = 0f;
            holdProgressFill.gameObject.SetActive(true);
        }

        float elapsed = 0f;
        while (elapsed < holdDuration)
        {
            elapsed += Time.deltaTime;
            if (holdProgressFill != null) holdProgressFill.fillAmount = elapsed / holdDuration;
            if (await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow()) return;
        }

        CancelHold();
        TryBuyWithAnimation();
    }

    #endregion

    #region Animation

    private void ScaleTo(Vector3 target)
    {
        transform.DOKill();
        transform.DOScale(target, 0.15f).SetEase(Ease.OutQuad);
    }

    private void TryBuyWithAnimation()
    {
        if (_onBuy == null)
        {
            Debug.LogWarning($"[ShopItem] Bind되지 않은 슬롯: {name}", this);
            return;
        }
        if (!_onBuy(Data)) return;

        isSold = true;
        canvasGroup.blocksRaycasts = false;
        _onHoverExit?.Invoke(Data);
        PlayBuyAnimation().Forget();
    }

    private async UniTaskVoid PlayBuyAnimation()
    {
        if (ShopCanvas == null || inventoryIconRect == null)
        {
            Debug.LogWarning($"[ShopItem] ShopCanvas 또는 inventoryIconRect 미연결: {name}", this);
            gameObject.SetActive(false);
            return;
        }

        var originalParent = transform.parent;
        var originalSibling = transform.GetSiblingIndex();
        var originalPos = transform.position;

        transform.DOKill();
        transform.SetParent(ShopCanvas, true);
        transform.SetAsLastSibling();

        var seq = DOTween.Sequence()
            .Join(transform.DOMove(inventoryIconRect.position, 0.45f).SetEase(Ease.InBack))
            .Join(transform.DOScale(originScale * flyEndScale, 0.45f).SetEase(Ease.InQuad))
            .Join(canvasGroup.DOFade(0.3f, 0.45f).SetEase(Ease.InQuad));
        await seq.AsyncWaitForCompletion();

        inventoryIconRect.DOKill(true);
        inventoryIconRect.DOPunchScale(Vector3.one * 0.3f, 0.3f);

        gameObject.SetActive(false);
        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSibling);
        transform.position = originalPos;
        transform.localScale = originScale;
        canvasGroup.alpha = 1f;
    }

    #endregion
}