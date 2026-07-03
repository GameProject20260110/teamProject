using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using TMPro;

public class CardRevealAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup cardGroup;
    [SerializeField] private RectTransform cardRect;
    [SerializeField] private TextMeshProUGUI GoldText;
    [SerializeField] private CanvasGroup button;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private float UpOffset = 40f;
    [SerializeField] private float UpDuration = 0.4f;
    [SerializeField] private float DownDuration = 0.4f;
    [SerializeField] private int buttonDelay = 1000;


#pragma warning disable CS4014

    public async UniTask Reveal()
    {
        UiController.instance.backGround.SetActive(true);

        GoldText.text = BattleDataManager.instance.currentRewardData.clearGold.ToString();
        cardGroup.alpha = 0f;
        Vector2 originalPos = cardRect.anchoredPosition;
        cardRect.anchoredPosition = originalPos;

        Sequence seq = DOTween.Sequence();
        seq.Join(cardGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad));
        seq.Join(
            cardRect.DOAnchorPosY(originalPos.y + UpOffset, UpDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    cardRect.DOAnchorPosY(originalPos.y, DownDuration)
                        .SetEase(Ease.OutBack);
                })
        );

        await seq.AsyncWaitForCompletion();

        await UniTask.Delay(buttonDelay);
        await button.DOFade(1f, 1f);
    }

    public void OnClick_UnRevealWrapper()
    {
        UnReveal().Forget();
    }

    public async UniTask UnReveal()
    {
        button.alpha = 0f;
        await UniTask.Delay(100);
        UiController.instance.backGround.SetActive(false);
        UiController.instance.resultUI.Hide();
        RewardPanelUI.instance?.Show(BattleDataManager.instance.currentRewardData);
    }

    [ContextMenu("Test")]
    private void Reveal1() => Reveal().Forget();

#pragma warning restore CS4014
}
