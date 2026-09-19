using UnityEngine.EventSystems;
using UnityEngine;

public class ItemDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private ItemCard currentCard;

    public void OnPointerEnter(PointerEventData eventData)
    {
        currentCard = eventData.pointerDrag?.GetComponent<ItemCard>();
        if (currentCard == null) return;
        currentCard.SetGlow(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentCard == null) return;
        currentCard.SetGlow(false);
        currentCard = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        ItemDragHandler drag = eventData.pointerDrag?.GetComponent<ItemDragHandler>();
        if (drag == null || currentCard == null) return;

        drag.OnDropped();
        currentCard.UseItem();
        currentCard = null;
    }
}
