using DG.Tweening;
using DG.Tweening.Core;
using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

// 팝업 공용 배경 - 검은 오버레이
public class PopupOverlayView : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SpriteRenderer dimOverlay;
    [SerializeField] private string sortingLayer = "Popup";
    [SerializeField] private int sortingOrder = 0;
    [SerializeField, Min(1f)] private float coverScale = 1.05f;

    public event Action OnClicked;

    private int _sortingLayerId;
    private bool _layerResolved;
    private Tween _fadeTween;
    private DOGetter<float> _alphaGetter;
    private DOSetter<float> _alphaSetter;

    private void Awake()
    {
        EnsureDelegates();
        HideImmediate();
    }

    public void Prepare()
    {
        if (dimOverlay == null) return;

        EnsureLayer();
        dimOverlay.sortingLayerID = _sortingLayerId;
        dimOverlay.sortingOrder = sortingOrder;

        if (dimOverlay.sprite == null) return;
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null || !cam.orthographic)
        {
            Debug.LogWarning("[PopupOverlayView] 정사영(orthorgraphic) 카메라가 필요합니다.");
            return;
        }

        float height = cam.orthographicSize * 2f * coverScale;
        float width = height * cam.aspect;
        Vector2 spriteSize = dimOverlay.sprite.bounds.size;
        Transform t = dimOverlay.transform;
        Vector3 parentScale = t.parent != null ? t.parent.lossyScale : Vector3.one;

        Vector3 camPos = cam.transform.position;
        t.position = new Vector3(camPos.x, camPos.y, t.position.z);
        t.localScale = new Vector3(MathUtility.SafeDivide(width, spriteSize.x * parentScale.x), MathUtility.SafeDivide(height, spriteSize.y * parentScale.y), 1f);
    }

    public async UniTask FadeToAsync(float targetAlpha, float duration, CancellationToken ct)
    {
        if (dimOverlay == null) return;
        EnsureDelegates();

        _fadeTween?.Kill();
        dimOverlay.enabled = true;
        if(duration <= 0f) { SetAlphaInstant(targetAlpha); return; }

        _fadeTween = DOTween.To(_alphaGetter, _alphaSetter, targetAlpha, duration).SetEase(Ease.OutQuad);
        await _fadeTween.WithCancellation(ct);
        if (targetAlpha <= 0f) dimOverlay.enabled = false;
    }

    public void HideImmediate()
    {
        _fadeTween?.Kill();
        SetAlphaInstant(0f);
    }

    private void SetAlphaInstant(float alpha)
    {
        if (dimOverlay == null) return;
        SetAlpha(alpha);
        dimOverlay.enabled = alpha > 0f;
    }

    private void SetAlpha(float alpha)
    {
        SpriteRendererUtility.SetAlpha(dimOverlay, alpha, disableWhenZero: false);
    }

    private void EnsureDelegates()
    {
        if (_alphaGetter != null) return;
        _alphaGetter = () => dimOverlay != null ? dimOverlay.color.a : 0f;
        _alphaSetter = SetAlpha;
    }

    private void EnsureLayer()
    {
        if (_layerResolved) return;
        _sortingLayerId = SortingLayerUtility.ResolveId(sortingLayer, nameof(PopupOverlayView));
        _layerResolved = true;
    }
}
