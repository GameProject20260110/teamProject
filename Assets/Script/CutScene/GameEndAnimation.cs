using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GameEndAnimation : MonoBehaviour
{
    [SerializeField] private Image panelImage;
    [SerializeField] private CanvasGroup[] uiElements;
    [SerializeField] private CanvasGroup GameCanvasGroup;
    [SerializeField] private float fillDuration = 0.8f;
    [SerializeField] private float maskDuration = 2f;
    [SerializeField] private int elementDelayMs = 50;

    private UiController _uiController;

    [Inject]
    public void Construct(UiController uiController)
    {
        _uiController = uiController;
    }

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
        if (!_uiController.backGround.activeSelf)
            _uiController.backGround.SetActive(true);
        panelImage.GetComponent<CanvasGroup>().alpha = 1f;
        transform.localScale = Vector3.zero;
        await transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack).AsyncWaitForCompletion();
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
        _uiController.backGround.SetActive(false);
    }
}
