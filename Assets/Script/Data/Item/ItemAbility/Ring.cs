using Microsoft.Win32.SafeHandles;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Ring")]
public class Ring : ItemSo
{
    public DiceGachaTable gacha;
    public bool CanUse()
    {
        var pm = PlayerManager.instance;
        var psm = PlayerShopManager.instance;

        for (int i = 0; i < psm.TempDices.Count; i++)
        {
            bool isSpecial = i < pm.SpecialSlots.Length && pm.SpecialSlots[i];
            bool isEmpty = psm.TempDices[i] == pm.defaultDice;
            if (isSpecial && isEmpty) return true;
        }
        if (pm.extraDice == null || pm.extraDice == pm.defaultDice)
            return true;

        return false;
    }
    public override void Consumable()
    {
        DiceData dice = gacha.Roll();
        var pm = PlayerManager.instance;
        var psm = PlayerShopManager.instance;

        for(int i = 0; i < psm.TempDices.Count; i++)
        {
            bool isSpecial = i < pm.SpecialSlots.Length & pm.SpecialSlots[i];
            bool isEmpty = psm.TempDices[i] == pm.defaultDice;
            if (isSpecial && isEmpty)
            {
                psm.TempDices[i] = dice;
                Transform slots = ShopUIController.instance.myDicePanel.transform.GetChild(i);
                slots.GetComponentInChildren<BuyDice>().ChangeDiceInfo(dice);
                return;
            }
        }
        pm.extraDice = dice;
        ShopUIController.instance.extraDiceSlot.GetComponentInChildren<BuyDice>().ChangeDiceInfo(dice);
    }
}
