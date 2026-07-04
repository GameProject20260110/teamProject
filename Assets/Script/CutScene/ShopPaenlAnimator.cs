using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;

public class ShopPanelAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject shopCanvas;
    [SerializeField] private Image panelImage;
    [SerializeField] private RectTransform ribbonRect;
    [SerializeField] private RectMask2D contentMask;
    [SerializeField] private CanvasGroup[] uiElements;
    [SerializeField] private CanvasGroup shopCanvasGroup;
    [SerializeField] private ShopUIManager shopUIController;

    [Header("Settings")]
    [SerializeField] private float panelDuration = 0.8f;
    [SerializeField] private float maskDuration = 2f;
    [SerializeField] private int elementDelayMs = 50;

    public async UniTask Show()
    {
        if (shopCanvasGroup != null)
        {
            shopCanvasGroup.interactable = false;
            shopCanvasGroup.blocksRaycasts = false;
        }

        foreach (var element in uiElements)
        {
            element.alpha = 0f;
        }
            

        shopUIController.Initialize();

        panelImage.fillAmount = 0f;

        RectTransform panelRect = panelImage.GetComponent<RectTransform>();
        ribbonRect.anchoredPosition = new Vector2(0, 500);
        float ribbonRectAfterPosition = ribbonRect.anchoredPosition.y - panelRect.rect.height + 30f;

        panelImage.DOFillAmount(1f, panelDuration).SetEase(Ease.OutQuad);
        ribbonRect.DOAnchorPosY(ribbonRectAfterPosition, panelDuration).SetEase(Ease.OutQuad);

        await UniTask.Delay((int)(panelDuration * 1000));

        foreach (var element in uiElements)
        {
            element.DOFade(1f, 0.3f);
            await UniTask.Delay(elementDelayMs);
        }

        if (shopCanvasGroup != null)
        {
            shopCanvasGroup.interactable = true;
            shopCanvasGroup.blocksRaycasts = true;
        }
    }
    

    public async UniTask Hide()
    {
        //GameManager.instance.diceManager.SetupDiceBoard();
        //UiController.instance.RefreshInventory();
        
        AudioManager.instance.PlayBgm("Battle", true);

        foreach (var element in uiElements)
        {
            element.alpha = 0f;
        }

        ribbonRect.DOAnchorPosY(500, 0.4f).SetEase(Ease.InBack);
        panelImage.DOFillAmount(0f, 0.4f).SetEase(Ease.InBack);
        await UniTask.Delay(400);
        
        //UiController.instance.backGround.SetActive(false);

        shopCanvas.SetActive(false);
    }
}