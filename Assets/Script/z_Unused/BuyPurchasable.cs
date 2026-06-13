//using DG.Tweening;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public abstract class BuyPurchasable<T> : MonoBehaviour, IBeginDragHandler, 
//    IDragHandler, IPointerEnterHandler, IPointerExitHandler, 
//    IPointerDownHandler, IPointerClickHandler, IEndDragHandler
//    where T : class
//{
//    // === Components ===
//    protected Image img;
//    protected RectTransform rect;
//    protected CanvasGroup canvasGroup;
//    protected Tween scaleTween;

//    // === State ===
//    [SerializeField] protected bool bought = false;
//    public bool inPointer = false;
//    public bool isDragged = false;

//    // === Drag ===
//    protected Transform canvas;
//    protected Transform previousParent;

//    // === Data ===
//    public T Data { get; protected set; }
//    public RectTransform descPosition;

//    // === Abstract ===
//    protected abstract string DropTag { get; }
//    protected abstract string SlotTag { get; }
//    protected abstract int GetCost();
//    protected abstract int GetSellPrice();
//    protected abstract string GetItemName();
//    protected abstract void OpenPopup();
//    protected abstract void OpenDescPopup();
//    protected abstract bool OnBuy();
//    protected abstract bool OnSell();
//    //protected abstract void OnSwap(BuyPurchasable<T> other);

//    // === virtual===
//    public virtual bool CanBeginDrag() => true;
//    protected virtual bool IsInvaildDrop(GameObject other, bool swap) => false;
//    protected virtual void OnDropSuccess(GameObject other) { }

//    protected virtual void ApplyData(T data)
//    {
//        scaleTween?.Kill();
//        transform.localScale = Vector3.one * 0.8f;
//        scaleTween = transform.DOScale(1f, 1f)
//            .SetEase(Ease.OutBack)
//            .SetUpdate(true);
//    }

//    protected void Awake()
//    {
//        img = GetComponent<Image>();
//        canvas = GameObject.FindGameObjectWithTag("ShopCanvas").transform;
//        rect = GetComponent<RectTransform>();
//        canvasGroup = GetComponent<CanvasGroup>();

//    }

//    #region Pointer Events

//    public void OnPointerDown(PointerEventData eventData)
//    {
//        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
//    }
        
//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        if (PopupManager.instance == null)
//            return;

//        inPointer = true;
//        OpenPopup();
//    }

//    public void OnPointerExit(PointerEventData eventData)
//    {
//        if (PopupManager.instance == null)
//            return;

//        inPointer = false;
//        PopupManager.instance.ClosePopup();
//    }

//    public void OnPointerClick(PointerEventData eventData)
//    {
//        if (eventData.button == PointerEventData.InputButton.Right && bought)
//        {
//            if(OnSell()) PopupManager.instance.ClosePopup();
//            return;
//        }
//        if(eventData.button == PointerEventData.InputButton.Middle)
//        {
//            OpenDescPopup();
//        }
        
//    }

//    #endregion



//    #region Drag Events

//    public void OnBeginDrag(PointerEventData eventData)
//    {
//        if (!CanBeginDrag())
//        {
//            eventData.pointerDrag = null;
//            isDragged = false;
//            return;
//        }
//        isDragged = true;
//        previousParent = transform.parent;

//        transform.SetParent(canvas);
//        transform.SetAsLastSibling();

//        canvasGroup.alpha = 0.6f;
//        canvasGroup.blocksRaycasts = false;

//        scaleTween?.Kill();

//        scaleTween = transform.DOScale(0.8f, 0.15f)
//            .SetEase(Ease.OutBack)
//            .SetUpdate(true);
//    }

//    public void OnDrag(PointerEventData eventData)
//    {
//        if (!isDragged) return;
//        rect.position = eventData.position;
//    }

//    public void OnEndDrag(PointerEventData eventData)
//    {
//        scaleTween?.Kill();
//        scaleTween = transform.DOScale(1f,1f)
//            .SetEase(Ease.OutBack)
//            .SetUpdate(true);


//        if (!bought) 
//            HandleUnboughtDrop(eventData);
//        else 
//            HandleBoughtDrop(eventData);

//        ResetDragState();
//    }

//    #endregion



//    #region Drop Handling

//    private void HandleUnboughtDrop(PointerEventData eventData)
//    {
//        var other = eventData.pointerCurrentRaycast.gameObject;

//        if (IsInvalidUnboughtDrop(other))
//        {
//            RevertToParent();
//            return;
//        }

//        if (OnBuy())
//        {
//            OnDropSuccess(other);
//            bought = true;
//            Destroy(gameObject);
//        }

//    }

//    private void HandleBoughtDrop(PointerEventData eventData)
//    {
//        var other = eventData.pointerCurrentRaycast.gameObject;

//        if (IsInvalidBoughtDrop(other))
//        {
            
//            RevertToParent();
//            return;
//        }

//        if (other.CompareTag(SlotTag))
//        {
            
//            if (IsInvaildDrop(other, true)) 
//            {
//                RevertToParent(); 
//                return; 
//            }

//            if(this is BuyItem)
//            {
//                var otherItem = other.GetComponentInChildren<BuyItem>(true);
//                otherItem.gameObject.SetActive(true);
//                //OnSwap(otherItem.GetComponent<BuyPurchasable<T>>());
//                gameObject.SetActive(false);
//            }
//            //else if(this is BuyDice) 
//                //OnSwap(other.GetComponent<BuyPurchasable<T>>());
//        }
//        else if (other.CompareTag(DropTag))
//        {
//            if (IsInvaildDrop(other, true))
//            {
//                RevertToParent();
//                return;
//            }
            
//            //OnSwap(other.GetComponent<BuyPurchasable<T>>());
//        }

//        RevertToParent();
//    }

//    #endregion



//    #region Vaildation

//    private bool IsInvalidUnboughtDrop(GameObject dropTarget)
//    {
//        return transform.parent == canvas
//            || PlayerShopManager.instance.TempGold < GetCost()
//            || IsInvaildDrop(dropTarget, false);
           
//    }

//    private bool IsInvalidBoughtDrop(GameObject dropTarget)
//    {
//        return dropTarget == null
//            || transform.parent == canvas
//            || !transform.parent.CompareTag(SlotTag);
//    }

//    #endregion



//    #region Helpers

//    protected void RevertToParent()
//    {
//        transform.SetParent(previousParent);
//        rect.position = previousParent.GetComponent<RectTransform>().position;
//    }

//    private void ResetDragState()
//    {
//        canvasGroup.alpha = 1f;
//        canvasGroup.blocksRaycasts = true;
//        isDragged = false;
//    }

//    #endregion
//}
