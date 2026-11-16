using UnityEngine;
using UnityEngine.Rendering;
using System;
using TMPro;
using System.Threading;
using DG.Tweening;
using DG.Tweening.Core;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

public class MulliganCardView : MonoBehaviour, IPointerClickHandler, IPoolCallbackReceiver
{
    [SerializeField] private CardUI cardUI;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private SpriteRenderer selectedBorder;
    [SerializeField] private SpriteRenderer selectGlow;
    [SerializeField] private SortingGroup sortingGroup;

    public int HandIndex { get; private set; } = -1;
    public bool IsSelected => _selected;
    public event Action<MulliganCardView> OnClicked;

    private SpriteRenderer[] _renderers;
    private TMP_Text[] _texts;
    private Vector3 _baseScale = Vector3.one;
    private Vector3 _restLocalPos;                       // 선택 해제시 돌아갈 자리
    private float _alpha = 1f;
    private float _groupAlpha = 1f;
    private float _highlight;
    private float _borderAlpha = 1f;
    private float _glowAlpha  = 0.35f;
    private bool _selected;

    private Tween _alphaTween;
    private Tween _groupAlphaTween;
    private Tween _highlightTween;
    private DOGetter<float> _alphaGetter;
    private DOSetter<float> _alphaSetter;
    private DOGetter<float> _groupAlphaGetter;
    private DOSetter<float> _groupAlphaSetter;
    private DOGetter<float> _highlightGetter;
    private DOSetter<float> _highlightSetter;

    private void Awake()
    {
        _baseScale = transform.localScale;

        Transform root = contentRoot != null ? contentRoot : transform;
        _renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        _texts = root.GetComponentsInChildren<TMP_Text>(true);

        if (sortingGroup == null && !TryGetComponent(out sortingGroup))
            sortingGroup = gameObject.AddComponent<SortingGroup>();

        _alphaGetter = () => _alpha;
        _alphaSetter = v => { _alpha = v; ApplyVisual(); };
        _groupAlphaGetter = () => _groupAlpha;
        _groupAlphaSetter = v => { _groupAlpha = v; ApplyVisual(); };
        _highlightGetter = () => _highlight;
        _highlightSetter = v => { _highlight = v; ApplyVisual(); };
    }

    // 패널 배치 시 1회 호출
    public void Setup(int handIndex, CardRuntime runtime, Vector3 localPos, float scale, int sortingLayerId, int sortingOrder, float borderAlpha, float glowAlpha)
    {
        HandIndex = handIndex;
        _borderAlpha = borderAlpha;
        _glowAlpha = glowAlpha;

        transform.DOKill();
        _restLocalPos = localPos;
        transform.localPosition = localPos;
        transform.localRotation = Quaternion.identity;
        transform.localScale = _baseScale * scale;

        if (sortingGroup != null)
        {
            sortingGroup.sortingLayerID = sortingLayerId;
            sortingGroup.sortingOrder = sortingOrder;
        }

        _groupAlphaTween?.Kill();
        _groupAlpha = 1f;

        SetCard(runtime);
        ClearSelectedInstant();
    }

    public void SetCard(CardRuntime runtime)
    {
        if (runtime == null || cardUI == null) return;
        cardUI.SetUp(runtime);
    }

    public void SetAlphaInstant(float alpha)
    {
        _alphaTween?.Kill();
        _alpha = alpha;
        ApplyVisual();
    }

    // 카드 퇴장 / 등장 페이드
    public async UniTask FadeToAsync(float target, float duration, CancellationToken ct)
    {
        _alphaTween?.Kill();
        if (duration <= 0f) { SetAlphaInstant(target); return; }

        _alphaTween = DOTween.To(_alphaGetter, _alphaSetter, target, duration).SetEase(Ease.OutQuad);
        await _alphaTween.WithCancellation(ct);
    }

    // 패널 닫힘 페이드
    public async UniTask FadeGroupToAsync(float target, float duration, CancellationToken ct)
    {
        _groupAlphaTween?.Kill();
        if(duration <= 0f) { _groupAlpha = target; ApplyVisual(); return; }

        _groupAlphaTween = DOTween.To(_groupAlphaGetter, _groupAlphaSetter, target, duration).SetEase(Ease.OutQuad);
        await _groupAlphaTween.WithCancellation(ct);
    }

    // 선택 강조
    public void SetSelected(bool selected, float raise, float duration)
    {
        _selected = selected;

        Vector3 target = _restLocalPos + (selected ? Vector3.up * raise: Vector3.zero);
        transform.DOKill();
        _highlightTween?.Kill();

        if(duration <= 0f)
        {
            transform.localPosition = target;
            _highlight = selected ? 1f : 0f;
            ApplyVisual();
            return;
        }

        transform.DOLocalMove(target, duration).SetEase(Ease.OutQuad);
        _highlightTween = DOTween.To(_highlightGetter, _highlightSetter, selected ? 1f : 0f, duration).SetEase(Ease.OutQuad);
    }

    public void ClearSelectedInstant()
    {
        SetSelected(false, 0f, 0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke(this);
    }

    private void ApplyVisual()
    {
        float a = _alpha * _groupAlpha;

        if(_renderers != null)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                SpriteRendererUtility.SetAlpha(_renderers[i], a, disableWhenZero: false);
            }
        }

        if(_texts != null)
        {
            for (int i = 0; i < _texts.Length; i++)
            {
                if (_texts[i] != null) _texts[i].alpha = a;
            }
        }

        SpriteRendererUtility.SetAlpha(selectedBorder, a * _highlight * _borderAlpha);
        SpriteRendererUtility.SetAlpha(selectGlow, a * _highlight * _glowAlpha);
    }

    public void OnRent()
    {

    }

    public void OnReturn()
    {
        transform.DOKill();
        _alphaTween?.Kill();
        _groupAlphaTween?.Kill();
        _highlightTween?.Kill();
        OnClicked = null;
        HandIndex = -1;
        _selected = false;
        _highlight = 0f;
        _alpha = 1f;
        _groupAlpha = 1f;
        transform.localScale = _baseScale;
        ApplyVisual();
    } 
}
