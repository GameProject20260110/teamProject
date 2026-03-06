using UnityEngine;
using UnityEngine.EventSystems;

public class BuyDice : BuyPurchasable<DiceData>
{
    public ItemSlot Slot;

    protected override string DropTag => "BuyDice";
    protected override string SlotTag => "MySlot";
    protected override int GetCost() => LuckyStone.CalcDiscount(Data.gold);
    protected override int GetSellPrice() => Data.sell;

    protected override void OpenPopup() =>
        PopupManager.instance.OpenPopup(Data, descPosition);

    protected override bool IsInvaildDrop() => !GetComponentInParent<ItemSlot>().hasSpecialSlot;

    public void UpdateDiceInfo(DiceData data, bool buy)
    {
        bought = buy;
        Slot = GetComponentInParent<ItemSlot>();
        descPosition = GetComponentsInChildren<RectTransform>(true)[1];
        ApplyData(data);
    }

    public void ChangeDiceInfo(DiceData data)
    {
        ApplyData(data);
        PlayerManager.instance.PushPlayerDices(data, Slot.slotIndex);
    }

    protected override void ApplyData(DiceData data)
    {
        Data = data;
        img.sprite = data.skin.GetSprite(1);
    }

    protected override void OnBuy()
    {
        Slot = GetComponentInParent<ItemSlot>();
        if (Slot == null && !Slot.hasSpecialSlot) return;

        PopupManager.instance.BuyItems(GetCost());
        PlayerManager.instance.PushPlayerDices(Data, Slot.slotIndex);
    }

    protected override void OnSell()
    {
        PlayerManager.instance.PullPlayerDices(Data, Slot.slotIndex);
        PopupManager.instance.SellItems(GetSellPrice());
    }

    protected override void OnSwap(BuyPurchasable<DiceData> other)
    {
        var otherDice = (BuyDice)other;
        DiceData tmp = otherDice.Data;
        otherDice.ApplyData(Data);
        ApplyData(tmp);
        PlayerManager.instance.PushPlayerDices(Data, Slot.slotIndex);
        PlayerManager.instance.PushPlayerDices(otherDice.Data, otherDice.Slot.slotIndex);
    }

    protected override void OnSlotMove()
    {
        PlayerManager.instance.PullPlayerDices(Data, Slot.slotIndex);
        Slot = GetComponentInParent<ItemSlot>();
        PlayerManager.instance.PushPlayerDices(Data, Slot.slotIndex);
    }
}

