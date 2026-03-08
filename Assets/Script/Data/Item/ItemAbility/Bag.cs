using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Bag")]
public class Bag : ItemSo
{
    private void OnEnable()
    {
        isConsumable = false;
    }
    public override void Consumable()
    {
        if (PlayerManager.instance == null) return;

        bool[] slots = PlayerManager.instance.SpecialSlots;

        for(int i = 0; i < slots.Length; i++)
        {
            if (!slots[i])
            {
                slots[i] = true;
                PlayerManager.instance.Save();
                return;
            }
        }
    }
}
