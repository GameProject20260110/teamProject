using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using VContainer;

public class CardAppearEffect : MonoBehaviour
{
    [Header("타겟")]
    [SerializeField] private RectTransform cardRect;
    [SerializeField] private CanvasGroup cardGroup; 

    [Header("줌 아웃 설정")]
    [SerializeField] private float startScale = 1.8f;
    [SerializeField] private float zoomDuration = 0.25f;

    [Header("흔들림 설정")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 12f;
    [SerializeField] private int shakeVibrato = 8;

    [Header("파티클")]
    [SerializeField] private ParticleSystem sparkleParticle;

    [Header("빛 플래시")]
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashDuration = 0.3f;

    [Header("텍스트")]
    [SerializeField] private CanvasGroup labelGroup;

    private Vector3 originalScale;
    private Sequence currentSequence;

    private AudioManager _audioManager;

    [Inject]
    public void Construct(BattleDataManager battleDataManager, AudioManager audioManager)
    {
        _audioManager = audioManager;
    }


    void Awake()
    {
        originalScale = cardRect.localScale;
        cardGroup.alpha = 0f;
        if (flashImage != null) SetAlpha(flashImage, 0f);
        if (sparkleParticle != null) sparkleParticle.Stop();
        if (labelGroup != null) labelGroup.alpha = 0f;
    }

    [ContextMenu("Test Play")]
    public void Play() => Play(null);

    public void Play(Action onComplete)
    {
        currentSequence?.Kill();
        cardRect.DOKill();

        cardRect.localScale = Vector3.one * startScale;
        cardGroup.alpha = 0f;
        if (labelGroup != null) labelGroup.alpha = 0f;

        if (sparkleParticle != null)
        {
            sparkleParticle.Stop();
            sparkleParticle.Clear();
        }

        currentSequence = DOTween.Sequence();

        currentSequence.Append(
            cardGroup.DOFade(1f, zoomDuration).SetEase(Ease.OutQuad)
        );
        currentSequence.Join(
            cardRect.DOScale(originalScale, zoomDuration).SetEase(Ease.OutBack)
        );
        currentSequence.Join(
            DOVirtual.DelayedCall(0f, () => _audioManager.PlaySfx("Character"))
        );

        if (flashImage != null)
        {
            currentSequence.Join(
                flashImage.DOFade(0.8f, flashDuration * 0.3f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                        flashImage.DOFade(0f, flashDuration * 0.7f).SetEase(Ease.InQuad)
                    )
            );
        }

        currentSequence.AppendCallback(() => PlayParticle());
        currentSequence.Append(
            cardRect.DOShakeAnchorPos(
                shakeDuration,
                new Vector2(shakeStrength, 0f),
                shakeVibrato,
                0f,
                false,
                true
            )
        );

        if (labelGroup != null)
        {
            currentSequence.Append(
                labelGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad)
            );
        }

        currentSequence.OnComplete(() => onComplete?.Invoke());
    }

    private void PlayParticle()
    {
        if (sparkleParticle == null) return;
        sparkleParticle.Stop();
        sparkleParticle.Clear();
        sparkleParticle.Play();
    }

    void OnDestroy()
    {
        currentSequence?.Kill();
        cardRect?.DOKill();
        cardGroup?.DOKill();
        flashImage?.DOKill();
        labelGroup?.DOKill();
    }

    private void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}