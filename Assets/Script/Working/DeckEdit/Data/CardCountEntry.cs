using System;

[Serializable]
public class CardCountEntry
{
    public int cardId;
    public int count;

    public CardCountEntry(int cardId, int count)
    {
        this.cardId = cardId;
        this.count = count;
    }
}
