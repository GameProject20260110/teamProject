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
    [SerializeField] private RectTransform vsTextRect;
    [SerializeField] private TextMeshProUGUI vsText;
    [SerializeField] private Image blueBackground;
    [SerializeField] private Image redBackground;
    [SerializeField] private Image playerCharacter;
    [SerializeField] private Image enemyCharacter;

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
    [SerializeField] private float screenShakeStrength = 15f;
    [SerializeField] private float screenShakeDuration = 0.3f;

    [Header("VS 텍스트")]
    [SerializeField] private float vsAppearDuration = 0.3f;
    [SerializeField] private float vsStartScale = 0.7f;
    [SerializeField] private float bounceDuration = 0.1f;

    [Header("캐릭터 / 배경")]
    [SerializeField] private float characterBackFadeInDuration = 1f;
    [SerializeField] private float characterFadeInDuration = 0.7f;
    [SerializeField] private float characterFadeInDelay = 0.2f;

    [Header("아웃트로")]
    [SerializeField] private float holdDuration = 2.0f;
    [SerializeField] private float outroFadeDuration = 0.4f;

    private Sequence currentSequence;

    void Awake()
    {
        if (visualsRoot != null) visualsRoot.SetActive(false);
        ResetDimmer();
    }

    public void Play() => Play(null);

    public void Play(Action onComplete)
    {
        KillCurrent();
        visualsRoot.SetActive(true);
        SetInitialState();

        currentSequence = DOTween.Sequence();

        // 1. dimmer
        currentSequence.Append(
            dimmer.DOFade(dimmerTargetAlpha, dimmerDuration).SetEase(Ease.OutQuad)
        );

        // 2. 창1
        currentSequence.Append(
            spear1Rect.DOAnchorPos(crossPoint, spear1Duration).SetEase(spearEase)
        );

        // 3. 창2
        currentSequence.AppendInterval(spearDelay);
        currentSequence.Append(
            spear2Rect.DOAnchorPos(crossPoint, spear2Duration).SetEase(spearEase)
        );

        // 4. 창 충돌 효과
        currentSequence.AppendCallback(() => OnSpearImpact());

        // 5-1. vs텍스트 위에서 아래로 떨어짐
        currentSequence.AppendInterval(0.1f);
        currentSequence.Append(
           vsText.DOFade(1f, 0.15f)
        );
        currentSequence.Join(
            vsTextRect.DOScale(Vector3.one, vsAppearDuration).SetEase(Ease.OutBack)
        );
        currentSequence.Join(
        DOVirtual.DelayedCall(0.15f, () => OnTextLand())
        );

        // 5-2. vs텍스트 바닥에 닿으며 튀어 오름
        currentSequence.Append(
            vsTextRect.DOScale(new Vector3(1.1f, 0.9f, 1f), bounceDuration * 0.5f).SetEase(Ease.OutQuad)
        );
        currentSequence.Append(
            vsTextRect.DOScale(Vector3.one, bounceDuration * 0.5f).SetEase(Ease.InQuad)
        );

        // 6. 캐릭터 페이드 인
        currentSequence.AppendInterval(characterFadeInDelay);
        currentSequence.Append(
            blueBackground.DOFillAmount(1f, characterBackFadeInDuration).SetEase(Ease.OutQuad)
        );
        currentSequence.Join(
            redBackground.DOFillAmount(1f, characterBackFadeInDuration).SetEase(Ease.OutQuad)
        );

        currentSequence.Append(
            playerCharacter.DOFade(1f, characterFadeInDuration).SetEase(Ease.OutQuad)
        );
        currentSequence.Join(
            enemyCharacter.DOFade(1f, characterFadeInDuration).SetEase(Ease.OutQuad)
        );


        // 7. 홀드
        currentSequence.AppendInterval(holdDuration);

        // 8. 아웃트로
        currentSequence.AppendCallback(() => PlayOutro());
        currentSequence.AppendInterval(outroFadeDuration);
        currentSequence.OnComplete(() =>
        {
            visualsRoot.SetActive(false);
            ResetDimmer();
            
            onComplete?.Invoke();
            
        });
    }

    public void SetEnemySprite(Sprite sprite)
    {
        if (enemyCharacter != null)
            enemyCharacter.sprite = sprite;
    }

    public void SetPlayerSprite(Sprite sprite)
    {
        if (playerCharacter != null)
            playerCharacter.sprite = sprite;
    }

    private void OnSpearImpact()
    {
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
    }

    private void OnTextLand()
    {
        RectTransform shakeTarget = visualsRoot.GetComponent<RectTransform>();
        if (shakeTarget != null)
        {
            Vector2 originalPos = shakeTarget.anchoredPosition;
            shakeTarget.DOShakeAnchorPos(screenShakeDuration, screenShakeStrength, 20, 90f, false, true)
                .OnComplete(() => shakeTarget.anchoredPosition = originalPos);
        }
    }

    private void PlayOutro()
    {
        float d = outroFadeDuration;
        dimmer.DOFade(0f, d).SetEase(Ease.InQuad);
        spear1Image.DOFade(0f, d).SetEase(Ease.InQuad);
        spear2Image.DOFade(0f, d).SetEase(Ease.InQuad);
        vsText.DOFade(0f, d).SetEase(Ease.InQuad);
        blueBackground.DOFade(0f, d).SetEase(Ease.InQuad);
        redBackground.DOFade(0f, d).SetEase(Ease.InQuad);
        playerCharacter.DOFade(0f, d).SetEase(Ease.InQuad);
        enemyCharacter.DOFade(0f, d).SetEase(Ease.InQuad);

    }

    private void SetInitialState()
    {
        spear1Rect.anchoredPosition = new Vector2(-spearOffscreenDistance, spearOffscreenDistance);
        SetAlpha(spear1Image, 1f);

        spear2Rect.anchoredPosition = new Vector2(spearOffscreenDistance, spearOffscreenDistance);
        SetAlpha(spear2Image, 1f);

        vsTextRect.anchoredPosition = Vector2.zero;
        vsTextRect.localScale = Vector3.one * vsStartScale;
        SetTMPAlpha(vsText, 0f);

        blueBackground.fillAmount = 0f;
        redBackground.fillAmount = 0f;
        SetAlpha(blueBackground, 1f);
        SetAlpha(redBackground, 1f);
        SetAlpha(playerCharacter, 0f);
        SetAlpha(enemyCharacter, 0f);
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
        vsTextRect?.DOKill();
        dimmer?.DOKill();
        spear1Image?.DOKill();
        spear2Image?.DOKill();
        vsText?.DOKill();
        blueBackground?.DOKill();
        redBackground?.DOKill();
        playerCharacter?.DOKill();
        enemyCharacter?.DOKill();
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
    private void TestRound1() => Play();

    [ContextMenu("Test Round 2")]
    private void TestRound2() => Play();
}
