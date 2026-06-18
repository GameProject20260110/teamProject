using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class BattlePanelAnimation : MonoBehaviour
{
    [SerializeField] private Image panelImage;
    [SerializeField] private RectTransform ribbonRect;
    [SerializeField] private CanvasGroup[] uiElements;

    [Header("Settings")]
    [SerializeField] private float panelDuration = 0.8f;
    [SerializeField] private int elementDelayMs = 50;

    [ContextMenu("Test Play")]
    public void Play() => Play(null);

    public void Play(Action onComplete)
    {
        PlayAsync(onComplete).Forget();
    }

    private async UniTask PlayAsync(Action onComplete)
    {
        ribbonRect.gameObject.SetActive(true);

        foreach (var element in uiElements)
            element.alpha = 0f;

        panelImage.fillAmount = 0f;

        RectTransform panelRect = panelImage.GetComponent<RectTransform>();
        ribbonRect.anchoredPosition = new Vector2(-50, -370);
        float ribbonTargetX = ribbonRect.anchoredPosition.x - panelRect.rect.width + 70f;

        panelImage.DOFillAmount(1f, panelDuration).SetEase(Ease.OutQuad);
        ribbonRect.DOAnchorPosX(ribbonTargetX, panelDuration).SetEase(Ease.OutQuad);

        await UniTask.Delay((int)(panelDuration * 1000));

        foreach (var element in uiElements)
        {
            element.DOFade(1f, 0.3f);
            await UniTask.Delay(elementDelayMs);
        }

        onComplete?.Invoke();
    }

    [ContextMenu("Test Hide")]
    public void PlayHide() => PlayHide(null);

    public void PlayHide(Action onComplete)
    {
        PlayHideAsync(onComplete).Forget();
    }

    private async UniTask PlayHideAsync(Action onComplete)
    {
        foreach (var element in uiElements)
            element.alpha = 0f;

        ribbonRect.DOAnchorPosX(-50, 0.4f).SetEase(Ease.InBack);
        panelImage.DOFillAmount(0f, 0.4f).SetEase(Ease.InBack);

        await UniTask.Delay(400);

        onComplete?.Invoke();
    }

    void OnDestroy()
    {
        panelImage?.DOKill();
        ribbonRect?.DOKill();
        if (uiElements != null)
            foreach (var element in uiElements)
                element?.DOKill();
    }
}