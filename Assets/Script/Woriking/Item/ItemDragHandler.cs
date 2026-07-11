using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ItemDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ItemCard itemCard;
    private RectTransform rect;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private RectTransform rootCanvasRect;

    private Transform originalParent;
    private Vector2 originalPosition;

    private int originalSiblingIndex;
    private bool isDragging;
    private bool isDropped;

    private void Awake()
    {
        itemCard = GetComponent<ItemCard>();
        rect = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        rootCanvasRect = rootCanvas.GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        itemCard.SetDragging(isDragging);

        DOTween.Kill(rect);
        DOTween.Kill(transform);

        isDropped = false;
        originalParent = transform.parent;
        originalPosition = itemCard.OriginalPosition;
        originalSiblingIndex = transform.GetSiblingIndex();

        transform.SetParent(rootCanvas.transform, false);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        UiController.Instance.ToggleItemDragPanel();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        rect.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        itemCard.SetDragging(false);

        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSiblingIndex);
        
        

        if (!isDropped)
        {
            UiController.Instance.ToggleItemDragPanel();
            ReturnToOrigin();
        }
            
        else
            Destroy(gameObject);
    }

    public void OnDropped()
    {
        isDropped = true;
        UiController.Instance.ToggleItemDragPanel();
    }

    private void ReturnToOrigin()
    {
        itemCard.SetReturning(true);
        rect.DOAnchorPos(originalPosition, 0.3f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => itemCard.SetReturning(false));
    }
}