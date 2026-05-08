using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;
using DG.Tweening;

public class DraggableDice : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler
{
    public PanelEffect panelEffect;

    public Shadow shadowEffect;
    public float dragScale = 1.2f;
    public float dragScaleDuration = 0.1f;

    private Transform _originalParent;
    private Vector3 _originalScale;
    private Canvas _rootCanvas;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Dice _dice;
    private bool _isDragging = false;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _dice = GetComponent<Dice>();
        _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (shadowEffect == null)
            shadowEffect = GetComponent<Shadow>();
        if (shadowEffect != null)
            shadowEffect.enabled = false;
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManager.instance.IsFirstRoll)
        {
            _isDragging = false;
            return;
        }

        _isDragging = true;
        _originalParent = transform.parent;
        _originalScale = transform.localScale;

        transform.SetParent(_rootCanvas.transform, true);
        transform.SetAsLastSibling();

        _canvasGroup.blocksRaycasts = false;

        transform.DOScale(_originalScale * dragScale, dragScaleDuration).SetEase(Ease.OutBack);
        if (shadowEffect != null)
            shadowEffect.enabled = true;

        DicePanelManager.instance?.OnDicePickUp(_dice); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rootCanvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        _rectTransform.localPosition = localPoint;

        panelEffect?.CheckHover(eventData.position, eventData.pressEventCamera);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        _isDragging = false;
        _canvasGroup.blocksRaycasts = true;

        if(shadowEffect != null) 
            shadowEffect.enabled = false;

        panelEffect?.ResetPanelScale(); 

        bool placed = DicePanelManager.instance?.OnDiceDrop(_dice, eventData) ?? false;

        if (!placed)
        {
            transform.SetParent(_originalParent, false);
            _rectTransform.localPosition = Vector3.zero;
            transform.DOScale(_originalScale, 0.15f);
            DicePanelManager.instance?.RestoreDiceLocation(_dice, _originalParent);
        }
        else
        {
            transform.DOScale(_originalScale, 0.2f);
        }
    }

    public void ReturnToOriginalSlot()
    {
        _isDragging = false;
        transform.SetParent(_dice.OriginalSlot, false);
        _rectTransform.localPosition = Vector3.zero;
        transform.DOScale(_originalScale, 0.15f);
    }
}
