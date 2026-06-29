using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TitleManager : MonoBehaviour
{
    [Header("배경")]
    public Image titleBackground;
    public Image newBackground;
    public Image darkOverlay;

    [Header("원형 전환")]
    public Material circleHoleMaterial;

    [Header("효과")]
    public Image flashOverlay;

    [Header("타이틀 UI")]
    public CanvasGroup titleGroup;

    [Header("버튼")]
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;

    void Start()
    {
        darkOverlay.color = new Color(0, 0, 0, 0);
        titleGroup.alpha = 1f;
        circleHoleMaterial.SetFloat("_Radius", 0f);

        startButton.onClick.AddListener(OnStartClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        if (MainOption.instance != null)
            MainOption.instance.SetSettingsButtonActive(false);
        if (AudioManager.instance != null)
            AudioManager.instance.PlayBgm("Title");

    }

    // 게임 시작
    void OnStartClicked()
    {
        startButton.interactable = false;
        settingsButton.interactable = false;
        quitButton.interactable = false;

        PlayTransition();
    }

    // 설정
    void OnSettingsClicked()
    {
        MainOption.instance.ToggleSettingsPanel();
    }

    // 종료
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

        // 작은 원이 생겨남
        seq.Append(DOTween.To(
            () => flashOverlay.rectTransform.sizeDelta,
            x => flashOverlay.rectTransform.sizeDelta = x,
            new Vector2(100f, 100f), 0.4f
        ).SetEase(Ease.OutCubic));
        seq.Join(flashOverlay.DOFade(0.9f, 0.3f));

        // 잠깐 유지
        seq.AppendInterval(0.2f);

        // 순간 사라짐 (빨려들어가는 느낌)
        seq.Append(DOTween.To(
            () => flashOverlay.rectTransform.sizeDelta,
            x => flashOverlay.rectTransform.sizeDelta = x,
            Vector2.zero, 0.25f
        ).SetEase(Ease.InCubic));
        seq.Join(flashOverlay.DOFade(0f, 0.25f));

        // 찰나의 정적
        seq.AppendInterval(0.1f);

        // 3단계: 구멍 폭발
        seq.Append(DOTween.To(
            () => circleHoleMaterial.GetFloat("_Radius"),
            x => circleHoleMaterial.SetFloat("_Radius", x),
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
    }
}