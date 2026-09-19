using System;

[Serializable]
public class DeckIndexEntry
{
    public string deckId;
    public string deckName;
    public long lastModifiedUnix;
    public int coverCardId = -1;
}
