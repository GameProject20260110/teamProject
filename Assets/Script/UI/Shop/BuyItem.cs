using UnityEngine;

public class BuyItem : BuyPurchasable<ItemSo>
{
    public ItemSlot Slot;

    protected override string DropTag => "BuyItem";
    protected override string SlotTag => "Inventory";
    protected override int GetCost() => LuckyStone.CalcDiscount(Data.gold);
    protected override int GetSellPrice() => Data.sell;
    protected override string GetItemName() => Data.itemName;

    protected override void OpenPopup() =>
        PopupManager.instance.OpenPopup(Data, descPosition);

    protected override bool IsInvaildDrop(GameObject other)
    {
        var slot = other.GetComponent<ItemSlot>();
        if (slot == null) return true;

        if (!slot.CompareTag(SlotTag)) return true; // SlotTag = "Inventory"

        var existingItem = slot.GetComponentInChildren<BuyItem>(true);
        if (existingItem != null && existingItem.gameObject.activeSelf)
            return true;

        return false;
    }
        

    public void UpdateInfo(ItemSo item, bool isBought)
    {
        bought = isBought;
        Slot = GetComponentInParent<ItemSlot>();
        descPosition = GetComponentsInChildren<RectTransform>(true)[1];
        ApplyData(item);
        if (Slot.GetComponentInParent<SlotUI>() == null) return;
        GetComponentInParent<SlotUI>().UpdateSlotUI(GetItemName(), GetCost());    
    }

    public void ChangeItemInfo(ItemSo item)
    {
        ApplyData(item);
    }

    protected override void ApplyData(ItemSo data)
    {
        Data = data;
        img.sprite = data.itemIcon;
    }

    protected override void OnDropSuccess(GameObject other)
    {
        var slot = other.GetComponent<ItemSlot>();
        var targetItem = slot.GetComponentInChildren<BuyItem>(true);
        targetItem.gameObject.SetActive(true);
        targetItem.ChangeItemInfo(Data);
    }

    protected override bool OnBuy()
    {
        Slot = GetComponentInParent<ItemSlot>();
        return PlayerShopManager.instance.TryPurchaseItem(Data, Slot.slotIndex);
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