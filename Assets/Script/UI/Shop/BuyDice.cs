using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuyDice : BuyPurchasable<DiceData>
{
    public ItemSlot Slot;

    protected override string DropTag => "BuyDice";
    protected override string SlotTag => "MySlot";
    protected override int GetCost() => LuckyStone.CalcDiscount(Data.gold);
    protected override int GetSellPrice() => Data.sell;
    protected override string GetItemName() => Data.abilityName;

    protected override void OpenPopup() =>
        PopupManager.instance.OpenPopup(Data, descPosition);


    #region Initialization

    public void UpdateDiceInfo(DiceData data, bool isBought)
    {
        bought = isBought;
        Slot = GetComponentInParent<ItemSlot>();
        descPosition = GetComponentsInChildren<RectTransform>(true)[1];
        ApplyData(data);

        var slotUI = Slot?.GetComponentInParent<SlotUI>();
        if (slotUI != null)
            slotUI.UpdateSlotUI(GetItemName(), GetCost());       
    }

    #endregion



    #region Data Managment

    public void ChangeDiceInfo(DiceData data) => ApplyData(data);

    protected override void ApplyData(DiceData data)
    {
        if(data == null) return;
        Data = data;
        img.sprite = data.skin.GetSprite(1);
    }

    #endregion



    #region Drag & Drop

    public override bool CanBeginDrag() => Data != null && Data.diceNum != 0;

    protected override void OnDropSuccess(GameObject other)
    {
        other.GetComponent<BuyDice>().ChangeDiceInfo(Data);
    }

    #endregion



    #region Validation

    protected override bool IsInvaildDrop(GameObject other, bool isswap)
    { 
        if (!GetComponentInParent<ItemSlot>().hasSpecialSlot || !other.transform.parent.CompareTag(SlotTag)) return true;
        return isswap ? false : other.GetComponent<BuyDice>().Data.diceNum != 0;

        //return isswap 
        //    ? other.GetComponent<BuyDice>().Data.diceNum == 0 
        //    : other.GetComponent<BuyDice>().Data.diceNum != 0;
    }

    #endregion



    #region Buy & Sell

    protected override bool OnBuy()
    {
        Slot = GetComponentInParent<ItemSlot>();
        if (Slot == null || !Slot.hasSpecialSlot) return false;

        return PlayerShopManager.instance.TryPurchaseDice(Data, Slot.slotIndex);
    }

    protected override bool OnSell()
    {
        if(Data.diceNum == 0) return false;

        PlayerShopManager.instance.SellDice(Data, Slot.slotIndex, GetSellPrice());
        ChangeDiceInfo(PlayerManager.instance.defaultDice);
        return true;
    }

    #endregion



    #region Swap & Move

    protected override void OnSwap(BuyPurchasable<DiceData> other)
    {
        var otherDice = (BuyDice)other;

        DiceData tmp = otherDice.Data;
        otherDice.ApplyData(Data);
        ApplyData(tmp);

        PlayerShopManager.instance.SetDiceAtSlot(Slot.slotIndex, Data);
        PlayerShopManager.instance.SetDiceAtSlot(otherDice.Slot.slotIndex, otherDice.Data);        
    }

    #endregion
}

