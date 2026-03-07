using UnityEngine;

public class BuyDice : BuyPurchasable<DiceData>
{
    public ItemSlot Slot;

    protected override string DropTag => "BuyDice";
    protected override string SlotTag => "MySlot";
    protected override int GetCost() => LuckyStone.CalcDiscount(Data.gold);
    protected override int GetSellPrice() => Data.sell;
    protected override string GetItemName() => Data.name;

    protected override void OpenPopup() =>
        PopupManager.instance.OpenPopup(Data, descPosition);

    protected override bool IsInvaildDrop() =>
        !GetComponentInParent<ItemSlot>().hasSpecialSlot;

    public void UpdateDiceInfo(DiceData data, bool isBought)
    {
        bought = isBought;
        Slot = GetComponentInParent<ItemSlot>();
        descPosition = GetComponentsInChildren<RectTransform>(true)[1];
        ApplyData(data);
    }

    public void ChangeDiceInfo(DiceData data)
    {
        ApplyData(data);
        PlayerShopManager.instance.TempDices[Slot.slotIndex] = data;
    }

    protected override void ApplyData(DiceData data)
    {
        Data = data;
        img.sprite = data.skin.GetSprite(1);
    }

    protected override void OnBuy()
    {
        Slot = GetComponentInParent<ItemSlot>();
        if (Slot == null || !Slot.hasSpecialSlot) return;

        bool success = PlayerShopManager.instance.TryPurchaseDice(Data, Slot.slotIndex);
        if (!success) RevertToParent();
    }

    protected override void OnSell()
    {
        PlayerShopManager.instance.SellDice(Data, Slot.slotIndex, GetSellPrice());
    }

    protected override void OnSwap(BuyPurchasable<DiceData> other)
    {
        var otherDice = (BuyDice)other;
        DiceData tmp = otherDice.Data;
        otherDice.ApplyData(Data);
        ApplyData(tmp);

        PlayerShopManager.instance.TempDices[Slot.slotIndex] = Data;
        PlayerShopManager.instance.TempDices[otherDice.Slot.slotIndex] = otherDice.Data;
    }

    protected override void OnSlotMove()
    {
        int prevIndex = Slot.slotIndex;
        Slot = GetComponentInParent<ItemSlot>();
        PlayerShopManager.instance.TempDices[prevIndex] = PlayerManager.instance.defaultDice;
        PlayerShopManager.instance.TempDices[Slot.slotIndex] = Data;
    }
}

