using UnityEngine;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/LuckyStone")]
public class LuckyStone : ItemSo
{
    [Range(0, 1f)]
    public float discountChance = 0.2f;
    public int discountAmount = 2;

    public static int CalcDiscount(int price)
    {
        if (PlayerManager.instance == null) return price;

        foreach(var item in PlayerManager.instance.items)
        {
            if(item is LuckyStone stone) 
            {
                if(Random.value < stone.discountChance)
                {
                    return Mathf.Max(1, price - stone.discountAmount);
                }
            }
        }
        return price;
    }
}
