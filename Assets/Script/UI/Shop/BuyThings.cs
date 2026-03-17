using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuyThings : MonoBehaviour, IBeginDragHandler, IDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Component")]
    protected Image img;
    protected RectTransform rect;
    protected CanvasGroup canvasGroup;

    [Header("State")]
    [SerializeField] protected bool bought = false;
    public bool inPointer = false;
    public bool isDragged = false;

    [Header("Drag References")]
    protected Transform canvas;
    protected Transform previousParent;
    
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Awake()
    {
        img = GetComponent<Image>();
        canvas = GameObject.FindGameObjectWithTag("ShopCanvas").transform;
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

    }

    public virtual void OnPointerEnter(PointerEventData eventData) => inPointer = true;
    public virtual void OnPointerExit(PointerEventData eventData) => inPointer = false;

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        isDragged = true;
        previousParent = transform.parent;

        transform.SetParent(canvas);
        transform.SetAsLastSibling();

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        rect.position = eventData.position;
    }
}
