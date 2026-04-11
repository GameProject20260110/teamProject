using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameEndAnimation : MonoBehaviour
{
    [SerializeField] private Image panelImage;
    [SerializeField] private CanvasGroup[] uiElements;
    [SerializeField] private CanvasGroup GameCanvasGroup;

    [SerializeField] private float fillDuration = 0.8f;
    [SerializeField] private float maskDuration = 2f;
    [SerializeField] private int elementDelayMs = 50;

    public async UniTask Show()
    {
        if (GameCanvasGroup != null)
        {
            GameCanvasGroup.interactable = false;
            GameCanvasGroup.blocksRaycasts = false;
        }

        foreach (var element in uiElements)
        {
            element.alpha = 0f;
        }

        if (!UiController.instance.backGround.activeSelf)
            UiController.instance.backGround.SetActive(true);

        //panelImage.fillAmount = 0f;

        panelImage.GetComponent<CanvasGroup>().alpha = 1f;

        transform.localScale = Vector3.zero;

        await transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack).AsyncWaitForCompletion();

        //await panelImage.DOFillAmount(1f, fillDuration)
        //    .SetEase(Ease.OutQuad)
        //    .AsyncWaitForCompletion();

        foreach (var element in uiElements)
        {
            element.DOFade(1f, 0.3f);
            await UniTask.Delay(elementDelayMs);
        }

        if (GameCanvasGroup != null)
        {
            GameCanvasGroup.interactable = true;
            GameCanvasGroup.blocksRaycasts = true;
        }
    }


    public async UniTask Hide()
    {
        foreach (var element in uiElements)
        {
            element.alpha = 0f;
        }
        await UniTask.Delay(400);
        UiController.instance.backGround.SetActive(false);
    }
}
