using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using System.Threading;
using UnityEngine;

// 카드 패널 1칸의 시각 표현(패널 이미지 / 테두리 강조 / 카드 놓일 중심점)만 담당
public class FieldSlotView : MonoBehaviour
{
    private enum HighlightMode { Off, Available, Target }

    [SerializeField] private SpriteRenderer frame;      
    [SerializeField] private SpriteRenderer highlight;  // 테두리 강조 
    [SerializeField] private Transform cardAnchor;      // 놓인 카드의 부모

    public Transform CardAnchor => cardAnchor != null ? cardAnchor : transform;

    private HighlightMode _mode;
    private float _highlight;                   // 실제 적용 중인 알파
    private Tween _highlightTween;              // 대상/꺼짐/번쩍임용 알파 트윈
    private DOGetter<float> _highlightGetter;   
    private DOSetter<float> _highlightSetter;

    // 
    private float _pulseAlpha;                  // FieldBoardView가 매 프레임(깜빡임 트윈 갱신 시) 넣어주는 공용 값
    private float _blendFrom;
    private float _blend = 1f;                  // 0 = 진입 순간 알파, 1 = 공용 깜빡임 값
    private Tween _blendTween;
    private DOGetter<float> _blendGetter;
    private DOSetter<float> _blendSetter;

    private void Awake()
    {
        _highlightGetter = () => _highlight;
        _highlightSetter = v => { _highlight = v; ApplyHighlight(); };
        _blendGetter = () => _blend;
        _blendSetter = v => { _blend = v; ApplyAvailable(); };
        Hide(0f);
    }

    // 월드 좌표가 패널 이미지 안인지(z 무시).
    public bool Contains(Vector3 worldPos)
    {
        if (frame == null) return false; // 예외 방지
        Bounds bounds = frame.bounds;
        worldPos.z = bounds.center.z;
        return bounds.Contains(worldPos);
    }

    // 패널 크기를 cardAnchor 로컬 유닛으로(카드 크기 맞춤용).
    public Vector2 GetAnchorLocalSize()
    {
        if (frame == null) return Vector2.zero; 
        Vector3 size = frame.bounds.size;
        Vector3 anchorScale = CardAnchor.lossyScale;
        return new Vector2(Mathf.Abs(MathUtility.SafeDivide(size.x, anchorScale.x)),
                           Mathf.Abs(MathUtility.SafeDivide(size.y, anchorScale.y)));
    }

    #region 강조 단계

    // 공용 깜빡임에 합류
    public void ShowAvailable(Color color, float fadeDuration)
    {
        if (_mode == HighlightMode.Available) return;
        KillTweens();
        _mode = HighlightMode.Available;
        SetColor(color);

        _blendFrom = _highlight;
        if (fadeDuration <= 0f) { _blend = 1f; ApplyAvailable(); return; }
        _blend = 0f;
        _blendTween = DOTween.To(_blendGetter, _blendSetter, 1f, fadeDuration).SetEase(Ease.OutQuad);
    }

    // 공용 깜빡임 값 수신(FieldBoardView 전용). 
    public void SetPulseAlpha(float alpha)
    {
        _pulseAlpha = alpha;
        if (_mode == HighlightMode.Available) ApplyAvailable();
    }

    //깜빡임에서 빠져 밝게 고정.
    public void ShowTarget(Color color, float alpha, float duration)
    {
        KillTweens();
        _mode = HighlightMode.Target;
        SetColor(color);
        TweenHighlight(alpha, duration);
    }

    public void Hide(float duration)
    {
        KillTweens();
        _mode = HighlightMode.Off;
        TweenHighlight(0f, duration);
    }

    // peak로 바로 켜고 duration 동안 꺼짐
    public UniTask PlayFlashAsync(Color color, float peakAlpha, float duration, CancellationToken ct)
    {
        KillTweens();
        _mode = HighlightMode.Off;
        SetColor(color);
        _highlight = peakAlpha;
        ApplyHighlight();
        if (duration <= 0f) { TweenHighlight(0f, 0f); return UniTask.CompletedTask; }

        _highlightTween = DOTween.To(_highlightGetter, _highlightSetter, 0f, duration).SetEase(Ease.InQuad);
        return _highlightTween.WithCancellation(ct);
    }

    #endregion

    private void TweenHighlight(float alpha, float duration)
    {
        if (duration <= 0f) { _highlight = alpha; ApplyHighlight(); return; }
        _highlightTween = DOTween.To(_highlightGetter, _highlightSetter, alpha, duration).SetEase(Ease.OutQuad);
    }

    private void ApplyAvailable()
    {
        _highlight = Mathf.Lerp(_blendFrom, _pulseAlpha, _blend);
        ApplyHighlight();
    }

    private void KillTweens()
    {
        _highlightTween?.Kill();
        _highlightTween = null;
        _blendTween?.Kill();
        _blendTween = null;
    }

    // RGB만 교체(알파는 _highlight가 담당).
    private void SetColor(Color color)
    {
        if (highlight == null) return;
        color.a = _highlight;
        highlight.color = color;
    }

    private void ApplyHighlight()
    {
        SpriteRendererUtility.SetAlpha(highlight, _highlight); // 0이면 그리기 자체를 끔
    }

    private void OnDestroy()
    {
        KillTweens();
    }
}