using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Shoes")]
public class Shoes : ItemSo
{
    public bool On = false;

    public override void Reusable()
    {
        if (!On)
        {
            ShopItem.instance.hasShoes = true;
            On = true;
        }
        else
        {
            ShopItem.instance.hasShoes = false;
            On = false;
        }
            
    }
}
