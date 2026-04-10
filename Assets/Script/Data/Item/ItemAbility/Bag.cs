using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Bag")]
public class Bag : ItemSo
{
    private void OnEnable()
    {
        isConsumable = true;
    }
    public bool CanUse()
    {
        if (PlayerManager.instance == null) return false;
        bool[] slots = PlayerManager.instance.SpecialSlots;
        for(int i = 0; i < slots.Length; i++)
        {
            if (!slots[i]) return true;
        }
        return false;
    }
    public override void Consumable()
    {
        if(PlayerManager.instance == null) return;
        bool[] slots = PlayerManager.instance.SpecialSlots;
        for(int i = 0; i < slots.Length; i++)
        {
            if (!slots[i])
            {
                slots[i] = true;
                PlayerManager.instance.tempExtraSlotsCount++;
                if (ShopUIController.instance != null)
                {
                    var diceSlot = ShopUIController.instance.myDicePanel.transform.GetChild(i).GetComponent<ItemSlot>();
                    diceSlot.SetSpecialSlot(true);
                }
                return;
            }
        }
    }
}
