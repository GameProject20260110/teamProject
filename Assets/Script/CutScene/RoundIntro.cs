using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class RoundIntro : MonoBehaviour
{
    [SerializeField] private CardAppearEffect RoundEffect;

    [Header("UI 요소")]
    [SerializeField] private GameObject visualsRoot;
    [SerializeField] private Image dimmer;
    [SerializeField] private RectTransform spear1Rect;      
    [SerializeField] private RectTransform spear2Rect;      
    [SerializeField] private Image spear1Image;
    [SerializeField] private Image spear2Image;
    [SerializeField] private RectTransform impactEffect;
    [SerializeField] private Image impactImage;
    [SerializeField] private RectTransform roundTextRect;
    [SerializeField] private TextMeshProUGUI roundText;

    [Header("Dimmer")]
    [SerializeField] private float dimmerTargetAlpha = 0.65f;
    [SerializeField] private float dimmerDuration = 0.4f;

    [Header("Spear 설정")]
    [SerializeField] private float spearOffscreenDistance = 1400f;  
    [SerializeField] private float spear1Duration = 0.3f;           
    [SerializeField] private float spear2Duration = 0.3f;
    [SerializeField] private float spearDelay = 0.3f;              
    [SerializeField] private Ease spearEase = Ease.OutQuint;        
    [SerializeField] private Vector2 crossPoint = Vector2.zero;     

    [Header("충돌 효과")]
    [SerializeField] private float impactScaleStart = 0f;
    [SerializeField] private float impactScaleEnd = 1.5f;
    [SerializeField] private float impactDuration = 0.3f;
    [SerializeField] private float screenShakeStrength = 15f;
    [SerializeField] private float screenShakeDuration = 0.3f;

    [Header("텍스트 낙하")]
    [SerializeField] private float textDropHeight = 1200f;
    [SerializeField] private float textDropDuration = 0.4f;
    [SerializeField] private Ease textDropEase = Ease.InQuad;
    [SerializeField] private float textDropDelay = 0.1f;            
    [SerializeField] private float bounceDuration = 0.15f;
    [SerializeField] private float bounceHeight = 35f;

    [Header("아웃트로")]
    [SerializeField] private float holdDuration = 1.0f;
    [SerializeField] private float outroFadeDuration = 0.4f;

    private Sequence currentSequence;
    private Vector2 textTargetPos;

    void Awake()
    {
        if (visualsRoot != null) visualsRoot.SetActive(false);
        ResetDimmer();
    }

    public void Play(int roundNumber) => Play(roundNumber, null);

    public void Play(int roundNumber, Action onComplete)
    {
        KillCurrent();
        roundText.text = $"Round {roundNumber}";
        visualsRoot.SetActive(true);
        SetInitialState();

        currentSequence = DOTween.Sequence();

        currentSequence.Append(
            dimmer.DOFade(dimmerTargetAlpha, dimmerDuration).SetEase(Ease.OutQuad)
        );

        currentSequence.Append(
            spear1Rect.DOAnchorPos(crossPoint, spear1Duration).SetEase(spearEase)
        );

        currentSequence.AppendInterval(spearDelay);
        currentSequence.Append(
            spear2Rect.DOAnchorPos(crossPoint, spear2Duration).SetEase(spearEase)
        );

        currentSequence.AppendCallback(() => OnSpearImpact());
        currentSequence.AppendInterval(textDropDelay);
        currentSequence.Append(
            roundTextRect.DOAnchorPos(textTargetPos, textDropDuration).SetEase(textDropEase)
        );

        currentSequence.AppendCallback(() => OnTextLand());
        currentSequence.Append(
            roundTextRect.DOAnchorPosY(textTargetPos.y + bounceHeight, bounceDuration * 0.5f)
                .SetEase(Ease.OutQuad)
        );


        currentSequence.Append(
            roundTextRect.DOAnchorPosY(textTargetPos.y, bounceDuration * 0.5f)
                .SetEase(Ease.InQuad)
        );
        currentSequence.Join(
            roundTextRect.DOScale(Vector3.one, bounceDuration).SetEase(Ease.OutBack)
        );


        currentSequence.AppendInterval(holdDuration);


        currentSequence.AppendCallback(() => PlayOutro());
        currentSequence.AppendInterval(outroFadeDuration);
        currentSequence.OnComplete(() =>
        {
            visualsRoot.SetActive(false);
            ResetDimmer();
            
            onComplete?.Invoke();
            
        });
    }

    private void OnSpearImpact()
    {
        if (impactEffect != null && impactImage != null)
        {
            impactEffect.gameObject.SetActive(true);
            
            Color c = impactImage.color;
            c.a = 1f;
            impactImage.color = c;

            Animator animator = impactEffect.GetComponent<Animator>();
            animator.Play("ImpactAnim", 0, 0f); // 처음부터 재생

            // 애니메이션 길이만큼 대기 후 비활성화
            DOVirtual.DelayedCall(animator.GetCurrentAnimatorStateInfo(0).length, () =>
            {
                impactEffect.gameObject.SetActive(false);
            });
        }

        spear1Rect.DOAnchorPos(crossPoint + new Vector2(5f, -5f), 0.05f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                spear1Rect.DOAnchorPos(crossPoint, 0.1f).SetEase(Ease.InQuad);
            });

        spear2Rect.DOAnchorPos(crossPoint + new Vector2(-5f, -5f), 0.05f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                spear2Rect.DOAnchorPos(crossPoint, 0.1f).SetEase(Ease.InQuad);
            });

        RectTransform shakeTarget = visualsRoot.GetComponent<RectTransform>();
        if (shakeTarget != null)
        {
            Vector2 originalPos = shakeTarget.anchoredPosition;
            shakeTarget.DOShakeAnchorPos(screenShakeDuration, screenShakeStrength, 20, 90f, false, true)
                .OnComplete(() => shakeTarget.anchoredPosition = originalPos);
        }
    }

    private void OnTextLand()
    {
        roundTextRect.DOScale(new Vector3(1.25f, 0.75f, 1f), 0.08f).SetEase(Ease.OutQuad);
    }

    private void PlayOutro()
    {
        float d = outroFadeDuration;
        dimmer.DOFade(0f, d).SetEase(Ease.InQuad);
        spear1Image.DOFade(0f, d).SetEase(Ease.InQuad);
        spear2Image.DOFade(0f, d).SetEase(Ease.InQuad);
        roundText.DOFade(0f, d).SetEase(Ease.InQuad);

    }

    private void SetInitialState()
    {
        textTargetPos = new Vector2(0f, 0f);

        spear1Rect.anchoredPosition = new Vector2(-spearOffscreenDistance, spearOffscreenDistance);
        SetAlpha(spear1Image, 1f);

        spear2Rect.anchoredPosition = new Vector2(spearOffscreenDistance, spearOffscreenDistance);
        SetAlpha(spear2Image, 1f);

        roundTextRect.anchoredPosition = new Vector2(0f, textDropHeight);
        roundTextRect.localScale = Vector3.one;
        SetTMPAlpha(roundText, 1f);

        if (impactEffect != null)
        {
            SetAlpha(impactImage, 0f);
        }
    }

    private void ResetDimmer()
    {
        if (dimmer != null) SetAlpha(dimmer, 0f);
    }

    private void KillCurrent()
    {
        currentSequence?.Kill();
        spear1Rect?.DOKill();
        spear2Rect?.DOKill();
        roundTextRect?.DOKill();
        dimmer?.DOKill();
        spear1Image?.DOKill();
        spear2Image?.DOKill();
        roundText?.DOKill();
        impactEffect?.DOKill();
        impactImage?.DOKill();
    }

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    private void SetTMPAlpha(TextMeshProUGUI tmp, float a)
    {
        if (tmp == null) return;
        Color c = tmp.color;
        c.a = a;
        tmp.color = c;
    }

    void OnDestroy() => KillCurrent();

    // 테스트
    [ContextMenu("Test Round 1")]
    private void TestRound1() => Play(1);

    [ContextMenu("Test Round 2")]
    private void TestRound2() => Play(2);
}
