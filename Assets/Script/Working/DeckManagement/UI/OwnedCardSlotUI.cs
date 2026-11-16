using DG.Tweening;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OwnedCardSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPoolCallbackReceiver
{
    [SerializeField] private CardVisualView visual;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Color depletedTint = new Color(0.55f, 0.55f, 0.55f, 1f);
    [SerializeField] private DeckManagementConfig config;

    private CardDragPayload _dragPayload;
    private OwnedCardListController _owner;
    private CardData _data;
    private Vector3 _baseScale;
    private bool __canDrag;

    private Graphic[] _tintTargets;
    private Color[] _baseColors;

    private void Awake()
    {
        _dragPayload = GetComponent<CardDragPayload>();
        _baseScale = transform.localScale;

        _tintTargets = GetComponentsInChildren<Graphic>(true).Where(g => g != (Graphic)countText).ToArray();
        _baseColors = new Color[_tintTargets.Length];
        for (int i = 0; i < _tintTargets.Length; i++)
            _baseColors[i] = _tintTargets[i].color;
    }

    public void SetUp(OwnedCardListController owner, CardData data, int remaining)
    {
        _owner = owner;
        _data = data;
        __canDrag = remaining > 0;

        visual?.SetCard(data);
        ApplyDepletedTint(!__canDrag);

        if (countText != null) countText.text = remaining.ToString();

        _dragPayload.Setup(CardDragSource.OwnedSlot, data.cardID, OnRightClickAdd);
        _dragPayload.SetDraggable(__canDrag);
    }

    // 잔여 0이면 프리팹 전체를 회색으로 전환
    private void ApplyDepletedTint(bool depleted)
    {
        for(int i = 0; i< _tintTargets.Length; i++)
        {
            if (_tintTargets[i] == null) continue;
            _tintTargets[i].color = depleted ? depletedTint : _baseColors[i];
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        _owner?.OnSlotClicked(_data);
    }

    private void OnRightClickAdd()
    {
        if (_data == null || !__canDrag) return;
        _owner.RequestAddCard(_data.cardID);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!__canDrag) return;
        float scale = config != null ? config.hoverScale : 1.08f;
        transform.DOKill();
        transform.DOScale(_baseScale * scale, 0.15f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData evnetData)
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
