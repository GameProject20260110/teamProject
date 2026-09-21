using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;


public enum CardDragSource { DeckSlot, OwnedSlot }

[RequireComponent(typeof(RectTransform))]
public class CardDragPayload : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private DeckManagementConfig config;

    public CardDragSource Source { get; private set; }
    public int CardId { get; private set; }
    public bool IsDragging => _isDragging;

    private RectTransform _rect;
    private Canvas _rootCanvas;
    private RectTransform _rootCanvasRect;
    private CanvasGroup _canvasGroup;

    private Transform _originalParent;
    private Vector2 _originalAnchoredPos;
    private Vector2 _originalSize;
    private int _originalSiblingIndex;

    private bool _isDragging;
    private bool _isDraggable = true;
    private bool _hasHoverTarget;
    private Vector2 _hoverTargetSize;

    private Action _onRightClick;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        _rootCanvas = parentCanvas != null ? parentCanvas.rootCanvas : null;
        _rootCanvasRect = _rootCanvas != null ? _rootCanvas.GetComponent<RectTransform>() : null;
        _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(CardDragSource source, int cardId, Action onRightClick)
    {
        Source = source;
        CardId = cardId;
        _onRightClick = onRightClick;
    }

    public void SetDraggable(bool draggable) => _isDraggable = draggable;

    public void SetHoverTargetSize(Vector2 size)
    {
        _hoverTargetSize = size;
        _hasHoverTarget = true;
    }

    public void ClearHoverTarget() => _hasHoverTarget = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_isDraggable || _rootCanvas == null) return;

        DOTween.Kill(_rect);
        _isDraggable = true;
        _hasHoverTarget = false;

        _originalParent = transform.parent;
        _originalAnchoredPos = _rect.anchoredPosition;
        _originalSize = _rect.sizeDelta;
        _originalSiblingIndex = transform.GetSiblingIndex();

        transform.SetParent(_rootCanvas.transform, true);
        transform.SetAsLastSibling();
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!_isDragging || _rootCanvasRect == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rootCanvasRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        _rect.localPosition = localPoint;

        Vector2 targetSize = _hasHoverTarget ? _hoverTargetSize : _originalSize;
        float lerpSpeed = config != null ? config.dragSizeLerpSpeed : 12f;
        _rect.sizeDelta = Vector2.Lerp(_rect.sizeDelta, targetSize, Time.unscaledDeltaTime * lerpSpeed);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;
        _hasHoverTarget = false;
        _canvasGroup.blocksRaycasts = true;

        gameObject.SetActive(true);

        transform.SetParent(_originalParent, true);
        transform.SetSiblingIndex(_originalSiblingIndex);

        float duration = config != null ? config.returnDuration : 0.3f;
        _rect.DOKill();
        _rect.DOAnchorPos(_originalAnchoredPos, duration).SetEase(Ease.OutQuad);
        DOTween.To(() => _rect.sizeDelta, v => _rect.sizeDelta = v, _originalSize, duration).SetEase(Ease.OutQuad);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            _onRightClick?.Invoke();
    }
}
