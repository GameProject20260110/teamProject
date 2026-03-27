using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    [Header("Components")]
    Image image;
    RectTransform rect;

    [Header("Slot Info")]
    public int slotIndex;
    public bool hasSpecialSlot = false;

    [Header("Visual")]
    [SerializeField] private Image specialSlotImage;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;


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



    #region Pointer Events

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!gameObject.CompareTag("BuySlot"))
            image.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!gameObject.CompareTag("BuySlot"))
            image.color = normalColor;
    }

    #endregion



    #region Drop Handler
   
    public void OnDrop(PointerEventData eventData) // BuyPurchasable.OnEndDrag 보다 먼저 실행
    {
        var dragObj = eventData.pointerDrag;
        if (dragObj == null) return;

        var checkDice = dragObj.GetComponent<BuyPurchasable<DiceData>>();
        var checkItem = dragObj.GetComponent<BuyPurchasable<ItemSo>>();

        if (checkDice != null && !checkDice.isDragged) return;
        if (checkItem != null && !checkItem.isDragged) return;
        
        var buyItem = dragObj.GetComponent<BuyItem>();
        if (buyItem != null && buyItem.Data is Ring ring && !ring.CanUse())
        {
            Debug.Log("주사위 슬롯과 여분 슬롯의 자리가 없습니다.");
            ShopItem.instance.notificationUI.Show("주사위 슬롯과 여분 슬롯의 자리가 없습니다.");
            return;
        }

        eventData.pointerDrag.transform.SetParent(transform);
        eventData.pointerDrag.GetComponent<RectTransform>().position = rect.position;
    }

    #endregion
}
