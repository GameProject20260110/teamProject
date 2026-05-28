using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        ItemDragHandler drag = eventData.pointerDrag?.GetComponent<ItemDragHandler>();
        ItemCard card = eventData.pointerDrag.GetComponent<ItemCard>();
        if (drag == null || card == null) return;

        drag.OnDropped();
        card.UseItem();
    }
}
