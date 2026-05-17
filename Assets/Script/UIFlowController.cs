using UnityEngine;

public class UIFlowController : MonoBehaviour
{
    [SerializeField] private CardRevealAnimator resultAnimator;

    public async void OnNextRoundButton()
    {
        await resultAnimator.UnReveal();
        RoundManager.instance.StartRound();
    }
}