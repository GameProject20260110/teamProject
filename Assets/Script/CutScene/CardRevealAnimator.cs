using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using TMPro;
using VContainer;

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

    private BattleDataManager _battleDataManager;
    private UiController _uiController;
    private RewardPanelUI _rewardPanelUI;

    [Inject]
    public void Construct(BattleDataManager battleDataManager, UiController uiController, RewardPanelUI rewardPanelUI)
    {
        _battleDataManager = battleDataManager;
        _uiController = uiController;
        _rewardPanelUI = rewardPanelUI;
    }

    private void Start()
    {
        SetCardVisible(false);
        SetButtonVisible(false);
    }

    private void SetCardVisible(bool visible)
    {
        cardGroup.alpha = visible ? 1f : 0f;
        cardGroup.interactable = visible;
        cardGroup.blocksRaycasts = visible;
    }

    private void SetButtonVisible(bool visible)
    {
        button.alpha = visible ? 1f : 0f;
        button.interactable = visible;
        button.blocksRaycasts = visible;
    }

#pragma warning disable CS4014
    public async UniTask Reveal()
    {
        _uiController.backGround.SetActive(true);
        GoldText.text = _battleDataManager.currentRewardData.clearGold.ToString();

        // 카드/버튼 초기 상태로 리셋
        SetCardVisible(false);
        SetButtonVisible(false);

        Vector2 originalPos = cardRect.anchoredPosition;
        cardRect.anchoredPosition = originalPos;

        Sequence seq = DOTween.Sequence();
        seq.Join(
            cardGroup.DOFade(1f, fadeDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    cardGroup.interactable = true;
                    cardGroup.blocksRaycasts = true;
                })
        );
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

        await button.DOFade(1f, 1f).AsyncWaitForCompletion();
        button.interactable = true;
        button.blocksRaycasts = true;
    }

    public void OnClick_UnRevealWrapper()
    {
        UnReveal().Forget();
    }

    public async UniTask UnReveal()
    {
        // 버튼 먼저 잠그고 페이드아웃 (중복 클릭 방지)
        button.interactable = false;
        button.blocksRaycasts = false;
        await button.DOFade(0f, 0.2f).AsyncWaitForCompletion();
        await cardGroup.DOFade(0f, 0.2f).AsyncWaitForCompletion();

        await UniTask.Delay(100);

        _uiController.backGround.SetActive(false);
        _rewardPanelUI?.Show(_battleDataManager.currentRewardData);
    }

    [ContextMenu("Test")]
    private void Reveal1() => Reveal().Forget();
#pragma warning restore CS4014
}
