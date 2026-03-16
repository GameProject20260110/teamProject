using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Shoes")]
public class Shoes : ItemSo
{
    public bool On = false;

    public override void Reusable()
    {
        if (!On)
        {
            On = true;
        }
        else
        {
            On = false;
        }
            
    }
}
