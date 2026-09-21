using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckEditSession
{
    private readonly PlayerCardCollection _cardCollection;
    private readonly CardDatabase _cardDatabase;
    private readonly DeckManagementConfig _config;

    public string DeckId { get; private set; }
    public string DeckName { get; private set; } = "»õ µ¦";

    public IReadOnlyList<int> SlotCardIds => _slotCardIds;

    private readonly List<int> _slotCardIds = new List<int>();
    private DeckData _snapshot;

    public event Action OnChanged;

    public int MaxCards => _config != null ? _config.maxCardsPerDeck : 20;
    public bool IsFull => _slotCardIds.Count >= MaxCards;

    public DeckEditSession(PlayerCardCollection cardCollection, CardDatabase cardDatabase, DeckManagementConfig config)
    {
        _cardCollection = cardCollection;
        _cardDatabase = cardDatabase;
        _config = config;
    }

    public void Load(DeckData deck)
    {
        _slotCardIds.Clear();

        if(deck == null)
        {
            DeckId = Guid.NewGuid().ToString("N");
            DeckName = "New Deck";
        }
        else
        {
            DeckId = deck.deckId;
            DeckName = deck.deckName;
            _slotCardIds.AddRange(deck.cardIds);
        }

        _snapshot = new DeckData(DeckId, DeckName) { cardIds = new List<int>(_slotCardIds) };
        OnChanged?.Invoke();
    }

    public int GetRemainingOwned(int cardId)
    {
        int owned = _cardCollection != null ? _cardCollection.GetOwnedCount(cardId) : 0;
        int used = _slotCardIds.Count(id => id == cardId);
        return Mathf.Max(0, owned - used);
    }

    public bool CanAddCard(int cardId)
    {
        if (IsFull) return false;
        return GetRemainingOwned(cardId) > 0;
    }

    public bool AddCard(int cardId)
    {
        if (!CanAddCard(cardId)) return false;
        _slotCardIds.Add(cardId);
        OnChanged?.Invoke();
        return true;
    }

    public bool RemoveOne(int cardId)
    {
        int index = _slotCardIds.LastIndexOf(cardId);
        if (index < 0) return false;
        _slotCardIds.RemoveAt(index);
        OnChanged?.Invoke();
        return true;
    }

    public void Rename(string newName)
    {
        DeckName = newName ?? "";
        OnChanged?.Invoke();
    }

    public bool IsDirty()
    {
        if (_snapshot == null) return _slotCardIds.Count > 0 || !string.IsNullOrEmpty(DeckName);
        if (_snapshot.deckName != DeckName) return true;
        return !_snapshot.hasSameCards(_slotCardIds);
    }

    public DeckData BuildDeckForSave()
    {
        return new DeckData(DeckId, DeckName) { cardIds = new List<int>(_slotCardIds) };
    }

    public void MarkSaved()
    {
        _snapshot = BuildDeckForSave();
        OnChanged?.Invoke();
    }

    public CardData GetCardData(int cardId) => _cardDatabase != null ? _cardDatabase.FindById(cardId) : null;

}
