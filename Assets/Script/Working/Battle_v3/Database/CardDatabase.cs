using UnityEngine;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Database/CardDatabase")]
public class CardDatabase : ScriptableObject
{
    public List<CardData> allCards = new List<CardData>();

    private Dictionary<int, CardData> _lookup;

    public CardData FindById(int cardID)
    {
        if (_lookup == null) BuildLookup();

        _lookup.TryGetValue(cardID, out var card);
        if (card == null) Debug.LogWarning($"[CardDatabase] cardId {cardID}에 해당하는 카드가 없음");

        return card;
    }

    private void BuildLookup()
    {
        _lookup = new Dictionary<int, CardData>();
        foreach(var card in allCards)
        {
            if (card == null) continue;

            if (_lookup.ContainsKey(card.cardID))
            {
                Debug.LogWarning($"[CardDatabase] cardID {card.cardID} 중복 : {card.name}");
                continue;
            }
            _lookup[card.cardID] = card;
        }
    }

    #region Map

    public CardData RandomPickCard()
    {
        int index = Random.Range(0, allCards.Count);
        return allCards[index];
    }

    public List<CardData> PickRandomCards(int count)
    {
        var shuffled = new List<CardData>(allCards);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
        return shuffled.Take(Mathf.Min(count, shuffled.Count)).ToList();
    }

    #endregion

#if UNITY_EDITOR
    [ContextMenu("모든 카드 데이터 다시 스캔")]
    private void RescanAllCards() 
    {
        string[] guids = AssetDatabase.FindAssets("t:CardData");
        allCards = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<CardData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(card => card != null)
            .OrderBy(card => card.cardID)
            .ToList();

       _lookup = null;
       EditorUtility.SetDirty(this);
    }
#endif
}
