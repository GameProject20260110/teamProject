using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ShopEntranceDirector : MonoBehaviour
{
    [Serializable]
    public class FoliageLayer
    {
        public RectTransform target;
        public Vector2 direction = Vector2.left;
        public float distance = 0.6f;
        public float delay;
        [NonSerialized] public Vector2 startPos;
    }

    [Header("Canvas")]
    [SerializeField] RectTransform canvasRect;
    [SerializeField] GameObject introRoot;
    [SerializeField] CanvasGroup introGroup;
    [SerializeField] GameObject shopRoot;
    [SerializeField] CanvasGroup shopGroup;

    [Header("1. 풀숲")]
    [SerializeField] FoliageLayer[] foliage;
    [SerializeField] float openDuration = 1.3f;
    [SerializeField] Ease openEase = Ease.InOutCubic;

    [Header("1. 빛")]
    [SerializeField] RawImage darkness;
    [SerializeField] Image glow;
    [SerializeField] RectTransform entrance;
    [Tooltip("클수록 구멍이 작음. 시작 = 거의 깜깜")]
    [SerializeField] float holeStartSize = 14f;
    [Tooltip("열린 상태. 작을수록 밝은 영역이 넓음")]
    [SerializeField] float holeOpenSize = 0.45f;
    [SerializeField] float lightDuration = 1.7f;
    [SerializeField] float glowMaxAlpha = 0.6f;
    [SerializeField] float glowStartScale = 0.35f;
    [SerializeField] float glowEndScale = 1f;

    [Header("2. 입장")]
    [SerializeField] Button treeButton;
    [SerializeField] RectTransform zoomRoot;
    [SerializeField] float zoomScale = 1.35f;
    [SerializeField] float zoomDuration = 0.9f;
    [SerializeField] float irisDuration = 0.8f;
    [SerializeField] float irisEndSize = 80f;
    [SerializeField] Image blackout;

    [Header("3. 상점")]
    [SerializeField] RawImage shopFade;
    [SerializeField] float revealDuration = 1.0f;
    [SerializeField] float revealFrom = -0.6f;
    [SerializeField] float revealTo = 0.6f;
    [SerializeField] RectTransform speechBubble;

    Vector2 entranceUV;
    float aspect;
    Vector3 zoomRootStartPos;
    Tween glowLoop;
    bool entering;
    Canvas shopCanvas;

    void Awake()
    {
        shopCanvas = shopRoot.GetComponent<Canvas>();
        foreach (var f in foliage) f.startPos = f.target.anchoredPosition;
        zoomRootStartPos = zoomRoot.localPosition;
        treeButton.onClick.AddListener(OnTreeClicked);
    }

    void OnEnable() => PlayIntroAsync(this.GetCancellationTokenOnDestroy()).Forget();

    public void ResetState()
    {
        DOTween.Kill(this);
        glowLoop?.Kill();
        entering = false;

        introRoot.SetActive(true);
        SetShopVisible(false);
        introGroup.blocksRaycasts = false;
        treeButton.interactable = false;

        foreach (var f in foliage) f.target.anchoredPosition = f.startPos;
        zoomRoot.localScale = Vector3.one;
        zoomRoot.localPosition = zoomRootStartPos;

        UpdateScreenInfo();
        darkness.gameObject.SetActive(true);
        SetHole(holeStartSize);

        glow.color = WithAlpha(glow.color, 0f);
        glow.rectTransform.localScale = Vector3.one * glowStartScale;

        blackout.color = WithAlpha(blackout.color, 0f);
        blackout.gameObject.SetActive(false);
    }

    public async UniTask PlayIntroAsync(CancellationToken ct)
    {
        ResetState();
        float w = canvasRect.rect.width;

        var seq = DOTween.Sequence().SetId(this).SetLink(gameObject);

        foreach (var f in foliage)
        {
            Vector2 to = f.startPos + f.direction.normalized * (f.distance * w);
            _ = seq.Insert(f.delay, f.target.DOAnchorPos(to, openDuration).SetEase(openEase));
        }

        _ = seq.Insert(0.1f, TweenHole(holeStartSize, holeOpenSize, lightDuration, Ease.OutCubic));
        _ = seq.Insert(0f, glow.DOFade(glowMaxAlpha, openDuration).SetEase(Ease.OutQuad));
        _ = seq.Insert(0f, glow.rectTransform.DOScale(glowEndScale, lightDuration).SetEase(Ease.OutCubic));

        await seq.ToUniTask(cancellationToken: ct);

        glowLoop = glow.DOFade(glowMaxAlpha * 0.75f, 1.6f)
                       .SetEase(Ease.InOutSine)
                       .SetLoops(-1, LoopType.Yoyo)
                       .SetLink(gameObject);

        introGroup.blocksRaycasts = true;
        treeButton.interactable = true;
    }

    void OnTreeClicked() => EnterShopAsync(this.GetCancellationTokenOnDestroy()).Forget();

    public async UniTask EnterShopAsync(CancellationToken ct)
    {
        if (entering) return;
        entering = true;
        treeButton.interactable = false;
        introGroup.blocksRaycasts = false;
        glowLoop?.Kill();

        UpdateScreenInfo();

        Vector3 c = zoomRoot.localPosition;
        Vector3 focus = zoomRoot.parent.InverseTransformPoint(entrance.position);
        float k = 1f;

        var seq = DOTween.Sequence().SetId(this).SetLink(gameObject);
        _ = seq.Append(DOTween.To(() => k, v =>
        {
            k = v;
            zoomRoot.localScale = Vector3.one * v;
            zoomRoot.localPosition = focus + (c - focus) * v;
        }, zoomScale, zoomDuration).SetEase(Ease.InOutSine));

        _ = seq.Insert(zoomDuration * 0.3f, TweenHole(holeOpenSize, irisEndSize, irisDuration, Ease.InCubic));
        _ = seq.Insert(zoomDuration * 0.3f, glow.DOFade(0f, irisDuration).SetEase(Ease.InQuad));

        blackout.gameObject.SetActive(true);
        _ = seq.Append(blackout.DOFade(1f, 0.12f));

        await seq.ToUniTask(cancellationToken: ct);
        await UniTask.Delay(150, cancellationToken: ct);

        shopFade.gameObject.SetActive(true);
        SetVerticalFade(revealFrom);
        if (speechBubble) speechBubble.localScale = Vector3.zero;
        SetShopVisible(true);
        shopGroup.blocksRaycasts = false;
        introRoot.SetActive(false);

        float y = revealFrom;
        await DOTween.To(() => y, v => { y = v; SetVerticalFade(v); }, revealTo, revealDuration)
                     .SetEase(Ease.InOutSine).SetId(this).SetLink(gameObject)
                     .ToUniTask(cancellationToken: ct);
        shopFade.gameObject.SetActive(false);

        if (speechBubble)
            await speechBubble.DOScale(1f, 0.35f).SetEase(Ease.OutBack)
                              .SetId(this).SetLink(gameObject).ToUniTask(cancellationToken: ct);

        shopGroup.blocksRaycasts = true;
        shopGroup.interactable = true;
        entering = false;
    }

    void SetShopVisible(bool visible)
    {
        if (!shopRoot.activeSelf) shopRoot.SetActive(true);
        if (shopCanvas) shopCanvas.enabled = visible;
        shopGroup.alpha = visible ? 1f : 0f;
        shopGroup.blocksRaycasts = visible;
        shopGroup.interactable = visible;
    }

    Tween TweenHole(float fromSize, float toSize, float duration, Ease ease)
    {
        float t = 0f;
        float a = 1f / fromSize, b = 1f / toSize;
        return DOTween.To(() => t, v => { t = v; SetHole(1f / Mathf.Lerp(a, b, v)); }, 1f, duration)
                      .SetEase(ease);
    }

    void SetHole(float size)
    {
        float h = size;
        float w = size * aspect;
        darkness.uvRect = new Rect(0.5f - entranceUV.x * w, 0.5f - entranceUV.y * h, w, h);
    }

    void SetVerticalFade(float y) => shopFade.uvRect = new Rect(0f, y, 1f, 1f);

    void UpdateScreenInfo()
    {
        Rect r = canvasRect.rect;
        aspect = r.width / r.height;
        Vector2 local = canvasRect.InverseTransformPoint(entrance.position);
        entranceUV = Rect.PointToNormalized(r, local);
    }

    static Color WithAlpha(Color c, float a) { c.a = a; return c; }
}