using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class TitleView : MonoBehaviour
{
    [Header("배경")]
    [SerializeField] private Image titleBackground;
    [SerializeField] private Image darkOverlay;

    [Header("원형 전환")]
    [SerializeField] private Material circleHoleMaterial;
    [SerializeField] private Material circleRevealMaterial;
    [SerializeField] private RawImage backgroundPanel;

    [Header("효과")]
    [SerializeField] private ParticleSystem convergeBurst;

    [Header("타이틀 UI")]
    [SerializeField] private CanvasGroup titleGroup;

    [Header("버튼")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    public event Action OnStartClicked;
    public event Action OnSettingsClicked;
    public event Action OnQuitClicked;

    void Awake()
    {
        darkOverlay.color = new Color(0, 0, 0, 0);
        titleGroup.alpha = 1f;
        circleHoleMaterial.SetFloat("_Radius", 0f);
        circleRevealMaterial.SetFloat("_Radius", 0f);
        backgroundPanel.material = circleRevealMaterial;

        startButton.onClick.AddListener(() => OnStartClicked?.Invoke());
        settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
        quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
    }

    public void SetButtonsInteractable(bool interactable)
    {
        startButton.interactable = interactable;
        settingsButton.interactable = interactable;
        quitButton.interactable = interactable;
    }

    public Sequence PlayTransitionSequence()
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
        seq.Append(darkOverlay.DOFade(1f, 1.5f).SetEase(Ease.InQuad));
        return seq;
    }

    void OnDestroy()
    {
        if (circleHoleMaterial != null) circleHoleMaterial.SetFloat("_Radius", 0f);
        if (circleRevealMaterial != null) circleRevealMaterial.SetFloat("_Radius", 0f);
        if (convergeBurst != null) convergeBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
