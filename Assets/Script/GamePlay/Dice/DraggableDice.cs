using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using DG.Tweening;

public class DraggableDice : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("그림자 (SpriteRenderer 기반 대체)")]
    public GameObject shadowObject;

    public float dragScale = 1.2f;
    public float dragScaleDuration = 0.1f;

    private Transform _originalParent;
    private Vector3 _originalScale;
    private BoxCollider2D _collider;
    private SortingGroup _sortingGroup;
    private int _originalSortingOrder;
    private const int DragSortingOrder = 1000;

    private Dice _dice;
    private bool _isDragging = false;
    public bool _isDraggable = false;
    private FloatingEffect _floatingEffect;

    private void Awake()
    {
        _dice = GetComponent<Dice>();
        _floatingEffect = GetComponentInChildren<FloatingEffect>();
        _collider = GetComponent<BoxCollider2D>();
        _sortingGroup = GetComponent<SortingGroup>();

        if (shadowObject != null)
            shadowObject.SetActive(false);
    }

    public void SetDraggable(bool isDraggable)
    {
        _isDraggable = isDraggable;
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        _isDragging = true;
        _originalParent = transform.parent;
        _floatingEffect?.StopFloating();
        _originalScale = transform.localScale;

        if (_collider != null)
            _collider.enabled = false; // 드래그 중 자기 자신이 raycast에 잡히지 않게

        if (_sortingGroup != null)
        {
            _originalSortingOrder = _sortingGroup.sortingOrder;
            _sortingGroup.sortingOrder = DragSortingOrder; // 맨 앞으로
        }

        transform.DOScale(_originalScale * dragScale, dragScaleDuration).SetEase(Ease.OutBack);

        if (shadowObject != null)
            shadowObject.SetActive(true);

        DicePanelManager.instance?.OnDicePickUp(_dice);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        if (eventData.pointerCurrentRaycast.isValid)
        {
            transform.position = eventData.pointerCurrentRaycast.worldPosition;
        }
        else
        {
            // 콜라이더 없는 빈 공간 위로 드래그될 때 대비한 안전장치
            Vector3 screenPoint = new Vector3(
                eventData.position.x,
                eventData.position.y,
                Camera.main.WorldToScreenPoint(transform.position).z
            );
            transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;

        if (_collider != null)
            _collider.enabled = true;

        if (_sortingGroup != null)
            _sortingGroup.sortingOrder = _originalSortingOrder;

        if (shadowObject != null)
            shadowObject.SetActive(false);

        bool placed = DicePanelManager.instance?.OnDiceDrop(_dice, eventData) ?? false;
        if (!placed)
        {
            transform.SetParent(_originalParent, false);
            transform.localPosition = Vector3.zero;
            transform.DOScale(_originalScale, 0.15f);
            DicePanelManager.instance?.RestoreDiceLocation(_dice, _originalParent);
        }
        else
        {
            transform.DOScale(_originalScale, 0.2f);
        }

        if (_floatingEffect != null) _floatingEffect.enabled = true;
    }

    public void ReturnToOriginalSlot()
    {
        _isDragging = false;
        transform.SetParent(_dice.OriginalSlot, false);
        transform.localPosition = Vector3.zero;
        transform.DOScale(_originalScale, 0.15f);
        if (_floatingEffect != null) _floatingEffect.enabled = true;
    }
}
