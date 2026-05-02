using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class RoundIntroController : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private GameObject rootObject;    
    [SerializeField] private RectTransform darkBand;   
    [SerializeField] private Image darkBandImage;      
    [SerializeField] private TextMeshProUGUI roundText;

    [Header("타이밍")]
    [SerializeField] private float bandFadeInDuration = 0.3f;
    [SerializeField] private float textFadeInDuration = 0.4f;
    [SerializeField] private float holdDuration = 0.6f;     
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float textFadeInDelay = 0.15f; 

    [Header("띠 설정")]
    [SerializeField] private float bandTargetHeight = 200f; 

    [Header("텍스트 설정")]
    [SerializeField] private float textStartScale = 0.7f;   
    [SerializeField] private float textEndScale = 1f;

    private Sequence currentSequence;

    void Awake()
    {
        if (rootObject != null)
        {
            rootObject.SetActive(false);
        }
    }

    public void Play(int roundNumber)
    {
        Play(roundNumber, null);
    }

    public void Play(int roundNumber, Action onComplete)
    {
        KillCurrent();
        roundText.text = $"Turn {roundNumber}";
        rootObject.SetActive(true);

        SetInitialState();
        currentSequence = DOTween.Sequence();

        currentSequence.Append(
            darkBand.DOSizeDelta(new Vector2(darkBand.sizeDelta.x, bandTargetHeight), bandFadeInDuration)
                .SetEase(Ease.OutQuad)
        );
        currentSequence.Join(
            darkBandImage.DOFade(0.85f, bandFadeInDuration).SetEase(Ease.OutQuad)
        );


        currentSequence.AppendInterval(textFadeInDelay);
        currentSequence.AppendCallback(() => {
            roundText.DOFade(1f, textFadeInDuration).SetEase(Ease.OutQuad);
            roundText.rectTransform.DOScale(textEndScale, textFadeInDuration).SetEase(Ease.OutBack);
        });

        currentSequence.AppendInterval(textFadeInDuration + holdDuration);

        currentSequence.AppendCallback(() => {
            roundText.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad);
            darkBandImage.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad);
            darkBand.DOSizeDelta(new Vector2(darkBand.sizeDelta.x, 0f), fadeOutDuration).SetEase(Ease.InQuad);
        });

        currentSequence.AppendInterval(fadeOutDuration);
        currentSequence.OnComplete(() => {
            rootObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    private void SetInitialState()
    {
        darkBand.sizeDelta = new Vector2(darkBand.sizeDelta.x, 0f);
        Color bandColor = darkBandImage.color;
        bandColor.a = 0f;
        darkBandImage.color = bandColor;

        roundText.rectTransform.localScale = Vector3.one * textStartScale;
        Color textColor = roundText.color;
        textColor.a = 0f;
        roundText.color = textColor;
    }

    private void KillCurrent()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }
        if (darkBand != null) darkBand.DOKill();
        if (darkBandImage != null) darkBandImage.DOKill();
        if (roundText != null)
        {
            roundText.DOKill();
            roundText.rectTransform.DOKill();
        }
    }

    public void Skip()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Complete();
        }
    }

    void OnDestroy()
    {
        KillCurrent();
    }

    // 테스트용
    [ContextMenu("Test Play Round 1")]
    private void TestPlay() => Play(1);
}
