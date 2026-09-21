using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class DeckData
{
    public string deckId;
    public string deckName;
    public List<int> cardIds = new List<int>();
    public long lastModifiedUnix;

    public DeckData() { }

    public DeckData(string deckId, string deckName)
    {
        this.deckId = deckId;
        this.deckName = deckName;
    }

    public DeckData Clone()
    {
        return new DeckData(deckId, deckName)
        {
            cardIds = new List<int>(cardIds),
            lastModifiedUnix = lastModifiedUnix
        };
    }

    public bool hasSameCards(IReadOnlyList<int> otherCardIds)
    {
        if (otherCardIds == null) return cardIds.Count == 0;
        if (cardIds.Count != otherCardIds.Count) return false;

        var a = cardIds.OrderBy(x => x).ToList();
        var b = otherCardIds.OrderBy(x => x).ToList();
        for (int i = 0; i < a.Count; i++)
            if (a[i] != b[i]) return false;

        return true;
    }
}
