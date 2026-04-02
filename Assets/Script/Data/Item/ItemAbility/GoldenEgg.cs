using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/GoldenEgg")]
public class GoldenEgg : ItemSo
{
    public int bonusGold = 1;
    public override void RoundEnd()
    {
        GameManager.instance.AddGold(bonusGold);
    }
}
