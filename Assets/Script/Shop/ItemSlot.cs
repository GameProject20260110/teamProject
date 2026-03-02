using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    
    Image image;
    RectTransform rect;
    public int slotIndex;

    [SerializeField] private Image specialSlotImage;
    public bool hasSpecialSlot { get; private set; } = false;

    void Awake()
    {
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
    }

    public void SetSpecialSlot(bool value)
    {
        hasSpecialSlot = value;
        if(specialSlotImage != null)
            specialSlotImage.gameObject.SetActive(value);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!gameObject.CompareTag("BuySlot"))
            image.color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!gameObject.CompareTag("BuySlot"))
            image.color = Color.white;
    }

    // BuyPurchasable.OnEndDrag 보다 먼저 실행
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            BuyThings buyThings = eventData.pointerDrag.GetComponent<BuyThings>();
            if (buyThings == null || !buyThings.isDragged) return;

            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.GetComponent<RectTransform>().position = rect.position;
        }
    }
}
