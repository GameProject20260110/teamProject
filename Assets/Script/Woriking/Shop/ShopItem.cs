using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Threading;

public abstract class ShopItem<T> : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    where T : class
{
    [Header("References")]
    [SerializeField] protected Image img;
    [SerializeField] protected RectTransform descPosition;
    //[SerializeField] private AudioClip PurchaseSound;
    [SerializeField] private string purchaseSoundKey;
    [SerializeField] protected TextMeshProUGUI nameText;
    [SerializeField] protected TextMeshProUGUI goldText;
    [SerializeField] private Image holdProgressFill;
    [SerializeField] private float holdDuration = 0.5f;

    public T Data { get; protected set; }

    protected bool isSold = false;
    private Vector3 originScale;
    private CancellationTokenSource _holdCts;

    // 인벤토리 아이콘 위치 (외부에서 연결)
    public RectTransform inventoryIconRect { protected get; set; }
    public Transform ShopCanvas { protected get; set; }

    protected abstract void ApplyData(T data);
    protected abstract bool OnBuy();
    protected abstract void OpenPopup();
    protected abstract void OpenDescPopup();

    public void Setup(T data)
    {
        isSold = false;
        gameObject.SetActive(true);
        originScale = transform.localScale;
        ApplyData(data);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    #region Pointer Events

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (PopupManager.instance == null) return;
        PlayPointerEnter().Forget();
        OpenPopup();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (PopupManager.instance == null) return;
        playPointerExit().Forget();
        PopupManager.instance.ClosePopup();
        CancelHold();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[ShopItem] OnPointerDown 호출됨, button :{eventData.button}, isSold: {isSold}");
        if (isSold || eventData.button != PointerEventData.InputButton.Left) return;
        _holdCts = new CancellationTokenSource();
        HoldToBuyAsync(_holdCts.Token).Forget();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelHold();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isSold) return;

        //if (eventData.button == PointerEventData.InputButton.Right)
        //{
        //    TryBuyWithAnimation();
        //    AudioManager.instance.PlaySfx(purchaseSoundKey);
        //}
            
        if (eventData.button == PointerEventData.InputButton.Middle)
            OpenDescPopup();
    }

    #endregion


    #region Hold To Buy

    private void CancelHold()
    {
        _holdCts?.Cancel();
        _holdCts?.Dispose();
        _holdCts = null;
        if(holdProgressFill != null)
        {
            holdProgressFill.fillAmount = 0f;
            holdProgressFill.gameObject.SetActive(false);
        }
    }

    private async UniTaskVoid HoldToBuyAsync(CancellationToken ct)
    {
        Debug.Log("[ShopItem] 홀딩 시작");

        if(holdProgressFill != null)
        {
            holdProgressFill.fillAmount = 0f;
            holdProgressFill.gameObject.SetActive(true);
        }

        float elapsed = 0f;
        while (elapsed < holdDuration)
        {
            if (ct.IsCancellationRequested)
            {
                Debug.Log("[ShopItem] 홀딩 취소됨");
                return;
            }
                elapsed += Time.deltaTime;
            if (holdProgressFill != null) holdProgressFill.fillAmount = elapsed / holdDuration;
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        if(ct.IsCancellationRequested) return;

        Debug.Log("[SHopItem] 홀딩 완료 -> 구매 시도");
        if(holdProgressFill != null) holdProgressFill.fillAmount = 0f;
        TryBuyWithAnimation();
        AudioManager.instance.PlaySfx(purchaseSoundKey);
    }

    #endregion

    #region Animation



    private void TryBuyWithAnimation()
    {
        if (!OnBuy()) return;

        isSold = true;
        PopupManager.instance.ClosePopup();
        PlayBuyAnimation().Forget();
    }

    private async UniTaskVoid PlayPointerEnter()
    {
        await transform.DOScale(originScale * 1.2f, 0.2f)
            .SetEase(Ease.Flash)
            .AsyncWaitForCompletion();
    }

    private async UniTaskVoid playPointerExit()
    {
        await transform.DOScale(originScale, 0.2f)
           .SetEase(Ease.Flash)
           .AsyncWaitForCompletion();
    }

    private async UniTaskVoid PlayBuyAnimation()
    {
        var originalParent = transform.parent;
        var originalPos = transform.position;

        transform.SetParent(ShopCanvas);
        transform.SetAsLastSibling();

        await transform.DOMove(inventoryIconRect.position, 0.4f)
            .SetEase(Ease.InBack)
            .AsyncWaitForCompletion();

        inventoryIconRect.DOPunchScale(Vector3.one * 0.3f, 0.3f);

        gameObject.SetActive(false);
        transform.SetParent(originalParent);
        transform.position = originalPos;
    }

    #endregion
}