using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupInputBlockView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Graphic blockGraphic;

    public event Action OnClicked;

    private bool _reportClicks;
    private float _acceptAfter;

    private void Awake()
    {
        SetBlocking(false);
    }

    public void SetBlocking(bool blocking)
    {
        if (blockGraphic != null) blockGraphic.raycastTarget = blocking;
        if (!blocking) _reportClicks = false;
    }

    public void SetClickReport(bool report, float ignoreDuration = 0f)
    {
        _reportClicks = report;
        _acceptAfter = Time.unscaledTime + Mathf.Max(0, ignoreDuration);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_reportClicks || Time.unscaledTime < _acceptAfter) return;
        OnClicked?.Invoke();
    }
}
