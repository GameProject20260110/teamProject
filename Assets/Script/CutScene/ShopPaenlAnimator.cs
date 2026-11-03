using Cysharp.Threading.Tasks;
using UnityEngine;

public class ShopPanelAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject shopCanvas;
    [SerializeField] private CanvasGroup shopCanvasGroup;
    [SerializeField] private ShopUIManager shopUIController;

    [Header("Settings")]
    [SerializeField] private float panelDuration = 0.8f;

    public async UniTask Show()
    {
        if (shopCanvasGroup != null)
        {
            shopCanvasGroup.interactable = false;
            shopCanvasGroup.blocksRaycasts = false;
        }

        shopUIController.Initialize();

        await UniTask.Delay((int)(panelDuration * 1000));

        if (shopCanvasGroup != null)
        {
            shopCanvasGroup.interactable = true;
            shopCanvasGroup.blocksRaycasts = true;
        }
    }


    public async UniTask Hide()
    {
        await UniTask.Delay(400);

    }
}