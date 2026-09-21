using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[RequireComponent(typeof(CardDragPayload))]
public class DeckCardSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPoolCallbackReceiver
{
    [SerializeField] private CardVisualView visual;
    [SerializeField] private DeckManagementConfig config;

    private CardDragPayload _dragPayload;
    private DeckGridController _owner;
    private CardData _data;
    private Vector3 _baseScale;

    private void Awake()
    {
        _dragPayload = GetComponent<CardDragPayload>();
        _baseScale = transform.localScale;
    }

    public void SetUp(DeckGridController owner, int cardId, CardData data)
    {
        _owner = owner;
        _data = data;

        if(data == null)
        {
            Debug.LogWarning($"[DeckCardSlotUI] cardId {cardId}에 해당하는 CardData가 없습니다.");
            visual?.Clear();
            _dragPayload?.Setup(CardDragSource.DeckSlot, cardId, OnRightClickRemove);
            return;
        }

        visual?.SetCard(data);
        _dragPayload.SetDraggable(true);
        _dragPayload.Setup(CardDragSource.DeckSlot, cardId, OnRightClickRemove);
        return;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        _owner?.OnSlotClicked(_data);
    }

    private void OnRightClickRemove()
    {
        if (_data == null) return;
        _owner?.RequestRemoveCard(_data.cardID);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        float scale = config != null ? config.hoverScale : 1.08f;
        transform.DOKill();
        transform.DOScale(_baseScale * scale, 0.15f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(_baseScale, 0.15f).SetEase(Ease.OutQuad);
    }

    public void OnRent() { }

    public void OnReturn()
    {
        transform.DOKill();
        transform.localScale = _baseScale;
        _owner = null;
        _data = null;
    }
}
