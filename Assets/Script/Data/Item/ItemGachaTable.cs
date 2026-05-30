using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemGachaTable", menuName = "Gacha/ItemGachaTable")]
public class ItemGachaTable : ScriptableObject
{
    [System.Serializable]
    public class ItemWeight
    {
        public BattleItemSo _item;
        public int weight;
    }

    public List<ItemWeight> items;

    public BattleItemSo Roll()
    {
        int total = 0;
        int weight = 0;
        int randNum = 0;

        foreach (var item in items)
        {
            total += item.weight;
        }
        randNum = Mathf.RoundToInt(total * Random.Range(0.0f, 1.0f));
        foreach (var item in items)
        {
            weight += item.weight;
            if (weight >= randNum)
            {
                return item._item;
            }
        }
        return null;
    }
}

