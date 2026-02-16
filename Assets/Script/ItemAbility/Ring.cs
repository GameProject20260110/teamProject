using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Ring")]
public class Ring : ItemSo
{
    public DiceGachaTable gacha;
    public override void Consumable()
    {
        DiceData dice = gacha.Roll();
        int randNum = Random.Range(0, Player.instance.player.DiceSo.Length);
        Transform slots = ShopItem.instance.myDicePanel.transform.GetChild(randNum);
        slots.GetComponentInChildren<BuyDice>().ChangeDiceInfo(dice);       
    }
}
