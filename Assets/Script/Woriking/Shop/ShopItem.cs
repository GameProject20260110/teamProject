using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ShopItem<T> : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    where T : class
{
    [Header("References")]
    [SerializeField] protected Image img;
    [SerializeField] protected RectTransform descPosition;
    [SerializeField] private AudioClip PurchaseSound;

    public T Data { get; protected set; }

    protected bool isSold = false;
    private Vector3 originScale;

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
        playPointerEnter().Forget();
        OpenPopup();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (PopupManager.instance == null) return;
        playPointerExit().Forget();
        PopupManager.instance.ClosePopup();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isSold) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            TryBuyWithAnimation();
            AudioManager.instance.PlaySfx(PurchaseSound);
        }
            
        if (eventData.button == PointerEventData.InputButton.Middle)
            OpenDescPopup();
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

    private async UniTaskVoid playPointerEnter()
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
