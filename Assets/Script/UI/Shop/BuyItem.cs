using UnityEngine;

public class BuyItem : BuyPurchasable<ItemSo>
{
    protected override string DropTag => "BuyItem";
    protected override string SlotTag => "Inventory";
    protected override int GetCost() => LuckyStone.CalcDiscount(Data.gold);
    protected override int GetSellPrice() => Data.sell;
    protected override string GetItemName() => Data.itemName;

    protected override void OpenPopup() =>
        PopupManager.instance.OpenPopup(Data, descPosition);
    protected override bool IsInvaildDrop(GameObject other)
    {
        if (other != null && other.CompareTag(DropTag)) return true;

        return !transform.parent.CompareTag(SlotTag);
    }
        

    public void UpdateInfo(ItemSo item, bool isBought)
    {
        bought = isBought;
        descPosition = GetComponentsInChildren<RectTransform>(true)[1];
        ApplyData(item);

        if (transform.parent.CompareTag(SlotTag)) return;
        GetComponentInParent<SlotUI>().UpdateSlotUI(GetItemName(), GetCost());    
    }

    protected override void ApplyData(ItemSo data)
    {
        Data = data;
        img.sprite = data.itemIcon;
    }

    protected override bool OnBuy()
    {
        return PlayerShopManager.instance.TryPurchaseItem(Data);       
    }

    protected override bool OnSell()
    {
        PlayerShopManager.instance.SellItem(Data, GetSellPrice());
        Destroy(gameObject);
        return true;
    }

    protected override void OnSwap(BuyPurchasable<ItemSo> other)
    {
        // 아이템 슬롯 간 순서 교환 — 골드 변동 없음
        var otherItem = (BuyItem)other;
        ItemSo tmp = otherItem.Data;
        otherItem.ApplyData(Data);
        ApplyData(tmp);
    }

    protected override void OnSlotMove() { }
}