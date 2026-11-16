using System.Collections.Generic;
using UnityEngine;

// 배틀에 쓸 플레이어 덱을 "저장된 덱(DeckRepository) -> CardData 목록"으로 변환하는 로직
public class BattleDeckController
{
    private readonly DeckRepository _deckRepository;
    private readonly CardDatabase _cardDatabase;

    public BattleDeckController(DeckRepository deckRepository, CardDatabase cardDatabase)
    {
        _deckRepository = deckRepository;
        _cardDatabase = cardDatabase;
    }

    public bool TryBuildBattleDeck(List<CardData> result, out string deckId)
    {
        deckId = null;
        if (result == null) return false;
        result.Clear();

        if (_deckRepository == null || _cardDatabase == null)
        {
            Debug.LogWarning("[BattleDeckController] DeckRepository 또는 CardDatabase가 없습니다.");
            return false;
        }

        deckId = ResolveDeckId();
        if(string.IsNullOrEmpty(deckId))
        {
            Debug.LogWarning("[BattleDeckController] 저장된 덱이 없습니다");
            return false;
        }

        DeckData deck = _deckRepository.LoadDeck(deckId);
        if (deck == null || deck.cardIds == null)
        {
            Debug.LogWarning($"[BattleDeckController] 덱을 불러오지 못했습니다.: {deckId}");
            return false;
        }

        foreach(int cardId in deck.cardIds)
        {
            CardData data = _cardDatabase.FindById(cardId);
            if (data != null) result.Add(data);
        }

        return result.Count > 0;
    }

    private string ResolveDeckId()
    {
        IReadOnlyList<DeckIndexEntry> summaries = _deckRepository.AllDeckSummaries;
        if (summaries == null || summaries.Count == 0) return null;

        string activeId = _deckRepository.ActiveDeckId;
        if(!string.IsNullOrEmpty(activeId))
        {
            for(int i = 0; i < summaries.Count; i++)
            {
                if (summaries[i] != null && summaries[i].deckId == activeId) return activeId;
            }
        }

        return summaries[0] != null ? summaries[0].deckId : null;
    }
}
