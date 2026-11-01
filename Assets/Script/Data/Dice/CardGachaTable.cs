using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "CardGachaTable", menuName = "Gacha/CardGachaTable")]
public class CardGachaTable : ScriptableObject
{
    public int level;

    [System.Serializable]
    public class CardWeight
    {
        public CardData card;
        public int weight;
    }

    public List<CardWeight> cardWeights = new();

    public CardData Roll()
    {
        int total = 0;
        foreach (var cw in cardWeights) total += cw.weight;
        if (total <= 0) return null;

        int rand = Random.Range(0, total);          // 0 ~ total-1
        int acc = 0;
        foreach (var cw in cardWeights)
        {
            acc += cw.weight;
            if (rand < acc) return cw.card;
        }
        return null;
    }

    /// 상점 슬롯 수만큼 중복 없이 뽑기
    public List<CardData> RollDistinct(int count)
    {
        var pool = cardWeights.Where(cw => cw.weight > 0 && cw.card != null).ToList();
        var result = new List<CardData>();

        while (result.Count < count && pool.Count > 0)
        {
            int total = pool.Sum(cw => cw.weight);
            int rand = Random.Range(0, total);
            int acc = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                acc += pool[i].weight;
                if (rand < acc)
                {
                    result.Add(pool[i].card);
                    pool.RemoveAt(i);
                    break;
                }
            }
        }
        return result;
    }
}
