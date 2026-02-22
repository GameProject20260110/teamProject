using UnityEngine;
using UnityEngine.EventSystems;

public class BuyItem : BuyPurchasable<ItemSo>
{
    protected override string DropTag => "BuyItem";
    protected override string SlotTag => "Inventory";
    protected override int GetCost() => Data.gold;
    protected override int GetSellPrice() => Data.sell;

    protected override void OpenPopup() =>
        PopupManager.instance.OpenPopup(Data, descPosition);

    public void UpdateInfo(ItemSo item, bool buy)
    {
        bought = buy;
        descPosition = GetComponentsInChildren<RectTransform>(true)[1];
        ApplyData(item);
    }

    protected override void ApplyData(ItemSo data)
    {
        Data = data;
        img.sprite = data.itemIcon;
    }

    protected override void OnBuy()
    {
        PlayerManager.instance.PushPlayerItems(Data);
        PopupManager.instance.BuyItems(GetCost());
    }
        

    protected override void OnSell()
    {
        PlayerManager.instance.PullPlayerItems(Data);
        PopupManager.instance.SellItems(GetSellPrice());
        Data.Reusable();
    }

    protected override void OnSwap(BuyPurchasable<ItemSo> other)
    {
        var otherItem = (BuyItem)other;
        ItemSo tmp = otherItem.Data;
        otherItem.ApplyData(Data);
        ApplyData(tmp);
    }

    protected override void OnSlotMove() { }
}