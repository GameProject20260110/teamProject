using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ItemWeight
{
    public BattleItemSo item;
    public int weight;
}

[CreateAssetMenu(fileName = "ItemWeightTable", menuName = "Reward/ItemWeightTable")]
public class ItemWeightTable : ScriptableObject
{
    public List<ItemWeight> items;

    public BattleItemSo GetRandomItem()
    {
        if (items == null || items.Count == 0) return null;

        int totalWeight = 0;
        foreach(var item in items)
            totalWeight += item.weight;

        int random = Random.Range(0, totalWeight);
        int current = 0;

        foreach(var item in items)
        {
            current += item.weight;
            if (random < current)
                return item.item;
        }
        return null;
    }
}
