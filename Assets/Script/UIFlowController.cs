using UnityEngine;

public class UIFlowController : MonoBehaviour
{
    [SerializeField] private CardRevealAnimator resultAnimator;
    [SerializeField] private PlayerShopManager shopManager;

    public async void OnNextRoundButton()
    {
        // 1. 결과 화면 닫기
        if (resultAnimator != null && resultAnimator.gameObject.activeSelf)
        {
            await resultAnimator.UnReveal();
        }

        // 2. 게임 로직
        if (RoundManager.instance != null)
        {
            RoundManager.instance.GoNextRound();
        }

        PlayerManager.instance.gameRerollCount = 1;
        PlayerManager.instance.isFirstRoll = true;

        // 3. 상점 열기
        shopManager.OpenWithAnimation();
    }
}