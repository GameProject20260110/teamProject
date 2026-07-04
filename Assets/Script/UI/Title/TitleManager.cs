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
    public Material circleHoleMaterial;
    public Material circleRevealMaterial;
    public RawImage backgroundPanel;

    [Header("효과")]
    public ParticleSystem convergeBurst; // 중심으로 모이는 파티클 (flashOverlay 대체)

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

        AudioManager.instance.PlaySfx("Click");
        AudioManager.instance.StopBgm();
        PlayTransition();
    }

    void OnSettingsClicked()
    {
        AudioManager.instance.PlaySfx("Click");
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
        seq.Append(titleGroup.DOFade(0f, 0.2f));

        seq.AppendCallback(() =>
        {
            if (convergeBurst != null)
            {
                convergeBurst.gameObject.SetActive(true);
                convergeBurst.Play();
            }
        });

        seq.AppendInterval(0.5f);

        seq.Append(DOTween.To(
            () => circleRevealMaterial.GetFloat("_Radius"),
            x => circleRevealMaterial.SetFloat("_Radius", x),
            2f, 1f
        ).SetEase(Ease.InExpo));

        seq.Append(darkOverlay.DOFade(1f, 1.5f)
            .SetEase(Ease.InQuad));

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

        if (convergeBurst != null)
            convergeBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}