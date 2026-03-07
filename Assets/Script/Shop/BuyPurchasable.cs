using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BuyPurchasable<T> : BuyThings, IPointerClickHandler, IEndDragHandler
    where T : class
{
    public T Data { get; protected set; }
    public RectTransform descPosition;

    protected abstract string DropTag { get; }
    protected abstract string SlotTag { get; }
    

    protected abstract int GetCost();
    protected abstract int GetSellPrice();
    protected abstract string GetItemName();
    protected abstract void ApplyData(T data);
    protected abstract void OpenPopup();
    protected abstract void OnBuy();
    protected abstract void OnSell();
    protected abstract void OnSwap(BuyPurchasable<T> other);
    protected abstract void OnSlotMove();

    // 포인터
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        OpenPopup();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        PopupManager.instance.ClosePopup();
    }

    // 클릭
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right && bought)
        {
            OnSell();
            PopupManager.instance.ClosePopup();
            Destroy(gameObject);
        }
    }

    //드래그
    public override void OnBeginDrag(PointerEventData eventData) => base.OnBeginDrag(eventData);
    public override void OnDrag(PointerEventData eventData) => base.OnDrag(eventData);

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!bought) HandleUnboughtDrop(eventData);
        else HandleBoughtDrop(eventData);
        ResetDragState();
    }

    protected virtual bool IsInvaildDrop() => false;

    private void HandleUnboughtDrop(PointerEventData eventData)
    {
        var other = eventData.pointerCurrentRaycast.gameObject;

        bool isInvalidDrop = transform.parent == canvas
            || PlayerShopManager.instance.TempGold - GetCost() < 0
            || (other != null && other.CompareTag(DropTag))
            || !transform.parent.CompareTag(SlotTag)
            || IsInvaildDrop();

        if (isInvalidDrop) { RevertToParent(); return; }

        bought = true;
        OnBuy();
    }

    private void HandleBoughtDrop(PointerEventData eventData)
    {
        var other = eventData.pointerCurrentRaycast.gameObject;

        if (other == null || transform.parent == canvas || !transform.parent.CompareTag(SlotTag))
        {
            RevertToParent();
            return;
        }

        if (other.CompareTag(DropTag))
        {
            OnSwap(other.GetComponent<BuyPurchasable<T>>());
            RevertToParent();
            return;
        }

        OnSlotMove();
    }

    protected void RevertToParent()
    {
        transform.SetParent(previousParent);
        rect.position = previousParent.GetComponent<RectTransform>().position;
    }

    private void ResetDragState()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        isDragged = false;
    }
}
