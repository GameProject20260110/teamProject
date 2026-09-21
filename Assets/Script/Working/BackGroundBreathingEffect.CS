using UnityEngine;
using DG.Tweening;

public class BackgroundBreathingEffect : MonoBehaviour
{
    [Header("Breathing Settings")]
    [Tooltip("커버 스케일 대비 최소 배율")]
    [SerializeField] private float minMultiplier = 1.0f;

    [Tooltip("커버 스케일 대비 최대 배율")]
    [SerializeField] private float maxMultiplier = 1.02f;

    [Tooltip("시간")]
    [SerializeField] private float duration = 18f;
    [SerializeField] private Ease easeType = Ease.InOutSine;
    [SerializeField] private bool playOnStart = true;

    private BackgroundCoverFitter _fitter;
    private Tween _breathingTween;
    private float _pulse;

    private void Awake()
    {
        _fitter = GetComponent<BackgroundCoverFitter>();
        if (_fitter == null) _fitter = BackgroundCoverFitter.Instance;
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartBreathing();
        }
    }

    public void StartBreathing()
    {
        _breathingTween?.Kill();
        _pulse = 0f;

        _breathingTween = DOTween.To(
            () => _pulse,
            v =>
            {
                _pulse = v;
                ApplyScale();
            },
            1f,
            duration)
        .SetEase(easeType)
        .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopBreathing()
    {
        _breathingTween?.Kill();
        ApplyScaleAtRest();
    }

    private void ApplyScale()
    {
        Vector3 cover = _fitter != null ? _fitter.CoverScale : Vector3.one;
        float mult = Mathf.Lerp(minMultiplier, maxMultiplier, _pulse);
        transform.localScale = new Vector3(cover.x * mult, cover.y * mult, cover.z);
    }

    private void ApplyScaleAtRest()
    {
        Vector3 cover = _fitter != null ? _fitter.CoverScale : Vector3.one;
        transform.localScale = cover;
    }

    private void OnDestroy()
    {
        _breathingTween?.Kill();
    }
}
