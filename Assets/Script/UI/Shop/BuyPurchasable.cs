using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BuyPurchasable<T> : BuyThings, IPointerClickHandler, IEndDragHandler
    where T : class
{
    public T Data { get; protected set; }
    public RectTransform descPosition;

    protected abstract string DropTag { get; }
    protected abstract string SlotTag { get; }
    
    // 데이터
    protected abstract int GetCost();
    protected abstract int GetSellPrice();
    protected abstract string GetItemName();
    protected abstract void ApplyData(T data);

    //액션
    protected abstract void OpenPopup();
    protected abstract bool OnBuy();
    protected abstract bool OnSell();
    protected abstract void OnSwap(BuyPurchasable<T> other);
    protected abstract void OnSlotMove(GameObject other);

    // 검증
    public virtual bool CanBeginDrag() => true;
    protected virtual bool IsInvaildDrop(GameObject other, bool swap) => false;
    protected virtual void OnDropSuccess(GameObject other) { }



    #region Pointer Events

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right && bought)
        {
            if(OnSell()) PopupManager.instance.ClosePopup();
        }
    }

    #endregion



    #region Drag Events

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanBeginDrag())
        {
            eventData.pointerDrag = null;
            isDragged = false;
            return;
        }
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (!isDragged) return;
        base.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!bought) 
            HandleUnboughtDrop(eventData);
        else 
            HandleBoughtDrop(eventData);

        ResetDragState();
    }

    #endregion



    #region Drop Handling

    private void HandleUnboughtDrop(PointerEventData eventData)
    {
        var other = eventData.pointerCurrentRaycast.gameObject;

        if (IsInvalidUnboughtDrop(other))
        {
            RevertToParent();
            return;
        }

        if (OnBuy())
        {
            OnDropSuccess(other);
            bought = true;
            Destroy(gameObject);
        }

    }

    private void HandleBoughtDrop(PointerEventData eventData)
    {
        var other = eventData.pointerCurrentRaycast.gameObject;

        if (IsInvalidBoughtDrop(other))
        {
            RevertToParent();
            return;
        }

        if (other.CompareTag(DropTag))
        {
            if (IsInvaildDrop(other, true)) 
            { 
                RevertToParent(); 
                return; 
            }
            OnSwap(other.GetComponent<BuyPurchasable<T>>());
        }
        else
        {
            OnSlotMove(other);
        }
            
        RevertToParent();
    }

    #endregion



    #region Vaildation

    private bool IsInvalidUnboughtDrop(GameObject dropTarget)
    {
        return transform.parent == canvas
            || PlayerShopManager.instance.TempGold < GetCost()
            || IsInvaildDrop(dropTarget, false);
    }

    private bool IsInvalidBoughtDrop(GameObject dropTarget)
    {
        return dropTarget == null
            || transform.parent == canvas
            || !transform.parent.CompareTag(SlotTag);
    }

    #endregion



    #region Helpers

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

    #endregion
}
