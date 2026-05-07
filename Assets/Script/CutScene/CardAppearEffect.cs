using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class CardAppearEffect : MonoBehaviour
{
    [Header("Å¸°Ù")]
    [SerializeField] private RectTransform cardRect;
    [SerializeField] private CanvasGroup cardGroup; 

    [Header("ÁÜ ¾Æ¿ô ¼³Á¤")]
    [SerializeField] private float startScale = 1.8f;
    [SerializeField] private float zoomDuration = 0.25f;

    [Header("Èçµé¸² ¼³Á¤")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 12f;
    [SerializeField] private int shakeVibrato = 8;

    [Header("ÆÄÆ¼Å¬")]
    [SerializeField] private ParticleSystem sparkleParticle;

    [Header("ºû ÇÃ·¡½Ã")]
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashDuration = 0.3f;

    private Vector3 originalScale;
    private Sequence currentSequence;

    void Awake()
    {
        originalScale = cardRect.localScale;
        cardGroup.alpha = 0f;
        if (flashImage != null) SetAlpha(flashImage, 0f);
        if (sparkleParticle != null) sparkleParticle.Stop();
    }

    [ContextMenu("Test Play")]
    public void Play() => Play(null);

    public void Play(Action onComplete)
    {
        currentSequence?.Kill();
        cardRect.DOKill();

        cardRect.localScale = Vector3.one * startScale;
        cardGroup.alpha = 0f;

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
    }

    private void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}