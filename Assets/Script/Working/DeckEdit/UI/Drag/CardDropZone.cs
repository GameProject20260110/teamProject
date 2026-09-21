using UnityEngine;
using UnityEngine.EventSystems;

public enum CardDropZoneKind { DeckGrid, OwnedBag }

public class CardDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CardDropZoneKind zoneKind;
    [SerializeField] private DeckGridController deckGrid;
    [SerializeField] private DeckManagementConfig config;

    private CardDragPayload _hoveringPayload;

    private Vector2 ZoneCardSize => config == null ? Vector2.zero : zoneKind == CardDropZoneKind.DeckGrid ? config.deckCardSize : config.ownedCardSize;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoveringPayload = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<CardDragPayload>() : null;
        _hoveringPayload?.SetHoverTargetSize(ZoneCardSize);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hoveringPayload?.ClearHoverTarget();
        _hoveringPayload = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        CardDragPayload payload = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<CardDragPayload>() : null;
        if (payload == null || deckGrid == null) return;

        if(zoneKind == CardDropZoneKind.DeckGrid && payload.Source == CardDragSource.OwnedSlot)
        {
            deckGrid.RequestAddCard(payload.CardId);
        }
        else if(zoneKind == CardDropZoneKind.OwnedBag && payload.Source == CardDragSource.DeckSlot)
        {
            deckGrid.RequestRemoveCard(payload.CardId);
        }

        _hoveringPayload = null;
    }
}
