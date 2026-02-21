using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuyThings : MonoBehaviour, IBeginDragHandler, IDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{

    protected Image img;
    protected bool bought = false;
    protected Transform canvas;
    [SerializeField]protected Transform previousParent;
    protected RectTransform rect;
    protected CanvasGroup canvasGroup;
    public bool inPotiner = false;
    public bool isDragged = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Awake()
    {
        img = GetComponent<Image>();
        canvas = FindObjectOfType<Canvas>().transform;
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();


    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        inPotiner = true;
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        inPotiner = false;
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        //if (isDragged == true) return;

        isDragged = true;
        previousParent = transform.parent;
        Debug.Log(previousParent);

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
