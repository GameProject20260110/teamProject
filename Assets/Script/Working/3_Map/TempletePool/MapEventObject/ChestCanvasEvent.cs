using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ChestCanvasEvent : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [Header("배경")]
    [SerializeField] private Image panel;

    [Header("상자")]
    [SerializeField] private RectTransform chestRect;
    [SerializeField] private Image chestImage;
    [SerializeField] private Sprite openChestSprite;
    [SerializeField] private Sprite closeChestSprite;
    [SerializeField] private CanvasGroup chestGroup;

    [Header("카드")]
    [SerializeField] private RectTransform[] cardRects;  
    [SerializeField] private RectTransform cardSpawnPoint;

    private CanvasGroup[] cardGroups;
    private Button[] cardButtons;
    private ChestCard[] cardUIs;

    private Vector2 chestRestPos;
    private Vector2[] cardLocalTarget;
    private Sequence sequence;

    private CardDatabase cardDatabase;
    private PlayerCardCollection cardCollection;
    private CardData[] pickedCards;

    [Inject]
    public void Construct(CardDatabase cardDatabase, PlayerCardCollection cardCollection)
    {
        this.cardDatabase = cardDatabase;
        this.cardCollection = cardCollection;
    }

    private void Awake()
    {
        chestRestPos = chestRect.anchoredPosition;

        cardLocalTarget = new Vector2[cardRects.Length];
        cardGroups = new CanvasGroup[cardRects.Length];
        cardButtons = new Button[cardRects.Length];
        cardUIs = new ChestCard[cardRects.Length];

        for (int i = 0; i < cardRects.Length; i++)
        {
            cardLocalTarget[i] = cardRects[i].anchoredPosition;
            cardGroups[i] = cardRects[i].GetComponent<CanvasGroup>();
            cardButtons[i] = cardRects[i].GetComponent<Button>();
            cardUIs[i] = cardRects[i].GetComponent<ChestCard>();
        }
        SetInitial();
    }

    public async UniTask PlayAsync()
    {
        SetInitial();

        pickedCards = cardDatabase.PickRandomCards(cardRects.Length).ToArray();
        for (int i = 0; i < cardRects.Length && i < pickedCards.Length; i++)
        {
            var data = pickedCards[i];
            cardUIs[i].Setup(data.diceSkin.GetSprite(1), data.cardName, data.description);
        }

        sequence = DOTween.Sequence();

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1f;

        _ = sequence.Append(panel.DOFade(180f / 255f, 0.5f));

        _ = sequence.Append(chestRect.DOAnchorPos(chestRestPos, 0.5f).SetEase(Ease.OutBounce));
        _ = sequence.AppendInterval(0.15f);

        _ = sequence.AppendCallback(() => chestImage.sprite = openChestSprite);

        float punchStartTime = sequence.Duration();
        _ = sequence.Insert(punchStartTime,chestRect.DOPunchScale(Vector3.one * 0.15f,0.25f));

        float cardsStartTime = punchStartTime;

        for (int i = 0;i< cardRects.Length; i++)
        {
            var card = cardRects[i];
            var target = cardLocalTarget[i];
            float startTime = cardsStartTime + i * 0.1f;

            _ = sequence.Insert(startTime, card.DOAnchorPos(target, 0.4f).SetEase(Ease.OutBack));
            _ = sequence.Insert(startTime, card.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
        }

        await sequence.AsyncWaitForCompletion();

        int selectedIndex = await WaitForCardClickAsync();

        canvasGroup.interactable = false;

        cardCollection.AddCard(pickedCards[selectedIndex].cardID, 1);

        var outro = DOTween.Sequence();
        for (int i = 0; i < cardGroups.Length; i++)
        {
            if (i == selectedIndex) continue;
            _ = outro.Join(cardGroups[i].DOFade(0f, 0.3f));
        }
        _ = outro.Join(chestGroup.DOFade(0f, 0.3f));
        _ = outro.Join(panel.DOFade(0f, 0.3f));

        await outro.AsyncWaitForCompletion();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0f;
    }

    private async UniTask<int> WaitForCardClickAsync()
    {
        var tasks = new UniTask<int>[cardButtons.Length];
        for (int i = 0; i < cardButtons.Length; i++)
        {
            int index = i;
            tasks[i] = WaitButtonClickAsync(cardButtons[index], index);
        }

        var (winnerIndex, _) = await UniTask.WhenAny(tasks);
        return winnerIndex;
    }

    private static async UniTask<int> WaitButtonClickAsync(Button button, int index)
    {
        await button.OnClickAsync();
        return index;
    }

    private void SetInitial()
    {
        var c = panel.color;
        panel.color = new Color(c.r, c.g, c.b, 0f);

        chestImage.sprite = closeChestSprite;
        chestRect.anchoredPosition = chestRestPos + new Vector2(0, 1300);
        chestRect.localScale = Vector3.one;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;

        chestGroup.alpha = 1f;

        for (int i = 0;i< cardRects.Length; i++)
        {
            cardRects[i].anchoredPosition = cardSpawnPoint.anchoredPosition;
            cardRects[i].localScale = Vector3.zero;
            cardGroups[i].alpha = 1f;
        }

    }

    private void OnDestroy() => sequence?.Kill();

    [ContextMenu("chestOpenCards")]
    public void testOpenChest() => PlayAsync().Forget();
}
