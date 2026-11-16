using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class ToastNotification : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private DeckManagementConfig config;

    private Sequence _sequence;

    private void Awake()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void ShowSuccess(string message, Action onComplete = null) => Show(message, onComplete);

    public void Show(string message, Action onComplete = null)
    {
        if (canvasGroup == null) { onComplete?.Invoke(); return; }
        if (messageText != null) messageText.text = message;

        float visibleDuration = config != null ? config.toastVisibleDuration : 1.2f;
        float fadeDuration = config != null ? config.toastFadeDuration : 0.25f;

        _sequence?.Kill();
        canvasGroup.alpha = 0f;
        gameObject?.SetActive(true);

        _sequence = DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, fadeDuration))
            .AppendInterval(visibleDuration)
            .Append(canvasGroup.DOFade(0f, fadeDuration))
            .OnComplete(() => 
            { 
                gameObject.SetActive(false); 
                onComplete?.Invoke();
            });

    }
}
