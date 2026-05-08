using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;

public class CardRevealAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image cardImage;
    [SerializeField] private RectMask2D contentMask;
    [SerializeField] private CanvasGroup[] buttons;

    [Header("Settings")]
    [SerializeField] private float fillDuration = 0.8f;
    [SerializeField] private float maskDuration = 2f;
    [SerializeField] private int buttonDelayMs = 500;

    public async UniTask Reveal()
    {
        UiController.instance.backGround.SetActive(true);

        cardImage.fillAmount = 0f;
        cardImage.GetComponent<CanvasGroup>().alpha = 1f;

        await cardImage.DOFillAmount(1f, fillDuration)
            .SetEase(Ease.OutQuad)
            .AsyncWaitForCompletion();

        contentMask.gameObject.SetActive(true);
        var maskRect = contentMask.GetComponent<RectTransform>();
        float targetHeight = maskRect.sizeDelta.y;
        maskRect.sizeDelta = new Vector2(maskRect.sizeDelta.x, 0f);

        await maskRect.DOSizeDelta(new Vector2(maskRect.sizeDelta.x, targetHeight), maskDuration)
            .SetEase(Ease.OutQuad)
            .AsyncWaitForCompletion();

        foreach (var btn in buttons)
        {
            await UniTask.Delay(buttonDelayMs);
            btn.DOFade(1f, 1f);
        }
    }

    public async UniTask UnReveal()
    {
        foreach (var btn in buttons)
        {
            btn.alpha = 0f;
        }

        await UniTask.Delay(100);
        UiController.instance.backGround.SetActive(false);
        UiController.instance.resultUI.Hide();
    }
}
