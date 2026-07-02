using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TitleManager : MonoBehaviour
{
    public static TitleManager instance;

    [Header("배경")]
    public Image titleBackground;
    public Image darkOverlay;

    [Header("원형 전환")]
    public Material circleHoleMaterial;    // TitleBack용 (기존 유지)
    public Material circleRevealMaterial;  // ✅ BackgroundPanel용 (새로 추가)
    public RawImage backgroundPanel;       // ✅ dark 배경 RawImage

    [Header("효과")]
    public Image flashOverlay;

    [Header("타이틀 UI")]
    public CanvasGroup titleGroup;

    [Header("버튼")]
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        darkOverlay.color = new Color(0, 0, 0, 0);
        titleGroup.alpha = 1f;
        circleHoleMaterial.SetFloat("_Radius", 0f);

        // ✅ BackgroundPanel 초기화 - 처음엔 안 보이게
        circleRevealMaterial.SetFloat("_Radius", 0f);
        backgroundPanel.material = circleRevealMaterial;

        startButton.onClick.AddListener(OnStartClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        if (MainOption.instance != null)
            MainOption.instance.SetSettingsButtonActive(false);
        if (AudioManager.instance != null)
            AudioManager.instance.PlayBgm("Title");
    }

    void OnStartClicked()
    {
        startButton.interactable = false;
        settingsButton.interactable = false;
        quitButton.interactable = false;
        AudioManager.instance.StopBgm();
        PlayTransition();
    }

    void OnSettingsClicked()
    {
        MainOption.instance.ToggleSettingsPanel();
    }

    void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void PlayTransition()
    {
        Sequence seq = DOTween.Sequence();

        // 1단계: 버튼 페이드 아웃
        seq.Append(titleGroup.DOFade(0f, 0.2f));

        // 2단계: 수축 → 폭발 직전 연출
        seq.AppendCallback(() =>
        {
            flashOverlay.gameObject.SetActive(true);
            flashOverlay.color = new Color(1f, 1f, 1f, 0f);
            flashOverlay.rectTransform.sizeDelta = Vector2.zero;
        });

        seq.Append(DOTween.To(
            () => flashOverlay.rectTransform.sizeDelta,
            x => flashOverlay.rectTransform.sizeDelta = x,
            new Vector2(100f, 100f), 0.4f
        ).SetEase(Ease.OutCubic));
        seq.Join(flashOverlay.DOFade(0.9f, 0.3f));

        seq.AppendInterval(0.2f);

        seq.Append(DOTween.To(
            () => flashOverlay.rectTransform.sizeDelta,
            x => flashOverlay.rectTransform.sizeDelta = x,
            Vector2.zero, 0.25f
        ).SetEase(Ease.InCubic));
        seq.Join(flashOverlay.DOFade(0f, 0.25f));

        seq.AppendInterval(0.1f);

        // 3단계: ✅ BackgroundPanel이 원으로 커지면서 등장 (altar, field 전부 덮음)
        seq.Append(DOTween.To(
            () => circleRevealMaterial.GetFloat("_Radius"),
            x => circleRevealMaterial.SetFloat("_Radius", x),
            2f, 1f
        ).SetEase(Ease.InExpo));

        // 4단계: 점점 어두워짐
        seq.Append(darkOverlay.DOFade(0.863f, 1.5f)
            .SetEase(Ease.InQuad));

        // 5단계: 씬 전환
        seq.AppendCallback(() =>
        {
            SceneController.instance.LoadMapFromTitle();
        });
    }

    void OnDestroy()
    {
        if (circleHoleMaterial != null)
            circleHoleMaterial.SetFloat("_Radius", 0f);
        if (circleRevealMaterial != null)
            circleRevealMaterial.SetFloat("_Radius", 0f);
    }
}