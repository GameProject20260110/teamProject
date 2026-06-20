using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class RewardIntroAnimator : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI clearTitle;
    [SerializeField] private float titleFadeDuration = 0.3f;

    [Header("카드")]
    [SerializeField] private float cardSlideUpDistance = 1500f;
    [SerializeField] private float slideUpDuration = 0.3f;
    [SerializeField] private float cardSlideInterval = 0.1f;

    [Header("건너뛰기")]
    [SerializeField] private CanvasGroup skipButton;
    [SerializeField] private float skipFadeInDuration = 0.3f;

    private List<RectTransform> _cardRects = new List<RectTransform>();

    public async UniTask PlayIntro(List<RewardCardUI> cards)
    {
        PlaceCardsAtSlots(cards);

        await PlayClearTitle();
        await PlayCardSlide(cards);
        await PlaySkipButton();
    }

    private void PlaceCardsAtSlots(List<RewardCardUI> cards)
    {
        _cardRects.Clear();

        for(int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == null) continue;

            RectTransform cardRect = cards[i].GetComponent<RectTransform>();
            _cardRects.Add(cardRect);
            cardRect.anchoredPosition = new Vector2(0, -cardSlideUpDistance);
        }
    }

    public async UniTask PlayClearTitle()
    {
        clearTitle.alpha = 0f;
        clearTitle.gameObject.SetActive(true);

        await clearTitle.DOFade(1f, titleFadeDuration).SetUpdate(true).SetLink(gameObject).ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

        await UniTask.Delay((int)(titleFadeDuration * 1000), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    public async UniTask PlayCardSlide(List<RewardCardUI> cards)
    {

        for (int i = 0; i < _cardRects.Count; i++)
        {
            int index = i;

            if (cards[index] != null)
            {
                cards[index].SetInteractable(false);
            }

            _cardRects[index].DOAnchorPos(Vector2.zero, slideUpDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(_cardRects[index].gameObject);

            await UniTask.Delay((int)(cardSlideInterval * 1000), cancellationToken: this.GetCancellationTokenOnDestroy());
        }
        await UniTask.Delay((int)(slideUpDuration * 1000), cancellationToken: this.GetCancellationTokenOnDestroy());

        foreach(var card in cards)
        {
            if(card != null) 
                card.SetInteractable(true);
        }
    }

    public async UniTask PlaySkipButton()
    {
        skipButton.alpha = 0f;
        skipButton.gameObject.SetActive(true);

        await skipButton.DOFade(1f, skipFadeInDuration)
            .SetLink(gameObject)
            .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    
}
