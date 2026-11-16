using System;
using System.Collections.Generic;

public class DrawController
{
    private readonly HanddrawConfig _config;
    private readonly HandData _hand;

    private readonly List<CardRuntime> _drawPile = new List<CardRuntime>();
    private readonly List<CardRuntime> _discardPile = new List<CardRuntime>();
    private readonly List<CardRuntime> _discardBuffer = new List<CardRuntime>(); // 멀리건 교체 시 임시 보관

    public int DrawPileCount => _drawPile.Count;
    public int DiscardPileCount => _discardPile.Count;

    public event Action<CardRuntime> OnCardDrawn;
    public event Action OnDeckReshuffled;

    public DrawController(HanddrawConfig config, HandData hand)
    {
        _config = config;
        _hand = hand;
    }

    public void InitializeDeck(IEnumerable<CardData> deckCards)
    {
        _drawPile.Clear();
        _hand.Clear();
        _discardPile.Clear();

        if (deckCards == null) return;

        foreach(CardData data in deckCards)
        {
            if (data == null) continue;
            _drawPile.Add(new CardRuntime(data));
        }

        Shuffle(_drawPile);
    }

    // 요청 매수 중 실제로 뽑을 수 있는 매수
    public int GetDrawableCount(int requested)
    {
        if (requested <= 0) return 0;
        return Math.Min(requested, _drawPile.Count + _discardPile.Count);
    }

    // 드로우 루프 도중 호출, 카드가 부족한지 판정
    public bool NeedsReshuffle() => _drawPile.Count == 0 && _discardPile.Count > 0;

    // 묘지에서 셔플해서 덱에 합류
    public void ReshuffleDiscardIntoDeck()
    {
        if (_discardPile.Count == 0) return;

        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
        Shuffle(_drawPile);
        OnDeckReshuffled?.Invoke();
    }

    public CardRuntime DrawOne()
    {
        if (_drawPile.Count == 0) return null;

        CardRuntime card = _drawPile[0];
        _drawPile.RemoveAt(0);
        _hand.Add(card);
        OnCardDrawn?.Invoke(card);
        return card;
    }

    // 멀리건 카드 교체

    public int ReplaceHandCards(IReadOnlyList<int> handIndices, List<CardRuntime> newCards)
    {
        if (newCards == null) return 0;
        newCards.Clear();
        if (handIndices == null) return 0;

        _discardBuffer.Clear();
        int replaced = 0;

        for(int i = 0; i < handIndices.Count; i++)
        {
            int handIndex = handIndices[i];
            bool canReplace = handIndex >= 0 && handIndex < _hand.Count && _drawPile.Count > 0;
            if(!canReplace)
            {
                newCards.Add(null);
                continue;
            }
            CardRuntime newCard = _drawPile[0];
            _drawPile.RemoveAt(0);

            _discardBuffer.Add(_hand[handIndex]);
            _hand.SetAt(handIndex, newCard);
            newCards.Add(newCard);
            replaced++;
        }

        _drawPile.AddRange(_discardBuffer);
        _discardBuffer.Clear();
        return replaced;
    }

    // 멀리건 종료 시점에 덱 1회 셔플
    public void ShuffleDrawPile() => Shuffle(_drawPile);

    private void Shuffle(List<CardRuntime> list)
    {
        for(int i = list.Count - 1; i > 0; i--)
        {
            int rand = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
