using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class RoundCharacter : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private GameObject rootObject;          // RoundIntro 루트
    [SerializeField] private RectTransform darkBand;         // 어두운 띠
    [SerializeField] private Image darkBandImage;            // 띠 이미지 (페이드용)
    [SerializeField] private TextMeshProUGUI vsText;

    [Header("타이밍")]
    [SerializeField] private float bandFadeInDuration = 0.3f;
    [SerializeField] private float textFadeInDuration = 0.4f;
    [SerializeField] private float holdDuration = 0.6f;     // 텍스트 머무는 시간
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float textFadeInDelay = 0.15f; // 띠가 펼쳐진 뒤 텍스트 등장

    [Header("띠 설정")]
    [SerializeField] private float bandTargetHeight = 200f; // 띠 최대 높이

    [Header("텍스트 설정")]
    [SerializeField] private float textStartScale = 0.7f;   // 줌 인 시작 스케일
    [SerializeField] private float textEndScale = 1f;

    private Sequence currentSequence;

    void Awake()
    {
        if (rootObject != null)
        {
            rootObject.SetActive(false);
        }
    }

    public void Play()
    {
        Play(null);
    }

    public void Play(Action onComplete)
    {
        KillCurrent();
        rootObject.SetActive(true);

        SetInitialState();
        currentSequence = DOTween.Sequence();

        // 1. 띠가 위아래로 펼쳐짐 (sizeDelta 변경)
        currentSequence.Append(
            darkBand.DOSizeDelta(new Vector2(darkBand.sizeDelta.x, bandTargetHeight), bandFadeInDuration)
                .SetEase(Ease.OutQuad)
        );
        // 띠 페이드 인 (동시에)
        currentSequence.Join(
            darkBandImage.DOFade(0.7f, bandFadeInDuration).SetEase(Ease.OutQuad)
        );

        // 2. 잠시 후 텍스트 등장 (페이드 인 + 줌 인 동시)
        currentSequence.AppendInterval(textFadeInDelay);
        currentSequence.AppendCallback(() => {
            // 텍스트 페이드 인
            vsText.DOFade(1f, textFadeInDuration).SetEase(Ease.OutQuad);
            // 텍스트 줌 인 (살짝 커지면서 안착)
            vsText.rectTransform.DOScale(textEndScale, textFadeInDuration).SetEase(Ease.OutBack);
        });

        // 3. 머무름
        currentSequence.AppendInterval(textFadeInDuration + holdDuration);

        // 4. 띠와 텍스트 동시에 페이드 아웃
        currentSequence.AppendCallback(() => {
            vsText.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad);
            darkBandImage.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad);
            // 띠도 다시 줄어듦
            darkBand.DOSizeDelta(new Vector2(darkBand.sizeDelta.x, 0f), fadeOutDuration).SetEase(Ease.InQuad);
        });

        currentSequence.AppendInterval(fadeOutDuration);

        // 5. 정리 + 콜백
        currentSequence.OnComplete(() => {
            rootObject.SetActive(false);

            
            // 카드 없으면 바로 외부 콜백
            onComplete?.Invoke();
            
        });
    }

    private void SetInitialState()
    {
        // 띠는 높이 0, 알파 0
        darkBand.sizeDelta = new Vector2(darkBand.sizeDelta.x, 0f);
        Color bandColor = darkBandImage.color;
        bandColor.a = 0f;
        darkBandImage.color = bandColor;

        // 텍스트는 작게, 알파 0
        vsText.rectTransform.localScale = Vector3.one * textStartScale;
        Color textColor = vsText.color;
        textColor.a = 0f;
        vsText.color = textColor;
    }

    private void KillCurrent()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }
        if (darkBand != null) darkBand.DOKill();
        if (darkBandImage != null) darkBandImage.DOKill();
        if (vsText != null)
        {
            vsText.DOKill();
            vsText.rectTransform.DOKill();
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
    private void TestPlay() => Play();
}