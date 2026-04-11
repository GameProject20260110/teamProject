using UnityEngine;

public class UIFlowController : MonoBehaviour
{
    [SerializeField] private CardRevealAnimator resultAnimator;
    [SerializeField] private PlayerShopManager shopManager;

    public async void OnNextRoundButton()
    {

        await resultAnimator.UnReveal();
         

        shopManager.OpenWithAnimation();
    }

   
}