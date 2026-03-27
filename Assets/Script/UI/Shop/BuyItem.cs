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

    protected override void OpenDescPopup() =>
        PopupManager.instance.DescOpenPopup(Data);
    
    public int GetTier() => Data.tier;

    #region Initialization

    public void UpdateInfo(ItemSo item, bool isBought)
    {
        bought = isBought;
        Slot = GetComponentInParent<ItemSlot>();
        descPosition = GetComponentsInChildren<RectTransform>(true)[1];
        ApplyData(item);

        var slotUI = Slot?.GetComponentInParent<SlotUI>();
        if (slotUI != null)
            slotUI.UpdateSlotUI(GetItemName(), GetCost());
    }

    #endregion



    #region Data Managment

    public void ChangeItemInfo(ItemSo item) => ApplyData(item);

    protected override void ApplyData(ItemSo data)
    {      
        Data = data;
        if(Data != null) img.sprite = data.itemIcon;
        base.ApplyData(data);
    }

    #endregion



    #region Validation

    protected override bool IsInvaildDrop(GameObject other, bool isswap)
    {
        if (other == null) return true;
        if (other.CompareTag(DropTag))
        {
            var otherItem = other?.GetComponent<BuyItem>();
            if (otherItem == null) return true;
            return false;
        }
        else if (other.CompareTag(SlotTag))
        {
            var slot = other?.GetComponent<ItemSlot>();
            if (slot == null || !slot.CompareTag(SlotTag)) return true;
            var existingItem = slot.GetComponentInChildren<BuyItem>(true);
            return existingItem != null && existingItem.gameObject.activeSelf;
        }
        return true;
        
    }

    #endregion



    #region Drop Success

    protected override void OnDropSuccess(GameObject other)
    {
        var slot = other.GetComponent<ItemSlot>();
        var targetItem = slot.GetComponentInChildren<BuyItem>(true);

        if (targetItem != null)
        {
            targetItem.gameObject.SetActive(true);
            targetItem.UpdateInfo(Data, true);
            if(Data is Ring)
            {
                targetItem.gameObject.SetActive(false);
            }
        }
    }

    #endregion



    #region Buy & Sell

    protected override bool OnBuy()
    {
        Slot = GetComponentInParent<ItemSlot>();
        if (Data is Ring ring && !ring.CanUse())
        {
            return false;
        }
        bool success = PlayerShopManager.instance.TryPurchaseItem(Data, Slot.slotIndex);
        if (success && Data.isConsumable) 
        {
            Data.Consumable();
            PlayerShopManager.instance.TempItems[Slot.slotIndex] = null;
        }
        return success;
    }

    protected override bool OnSell()
    {
        PlayerShopManager.instance.SellItem(Data, GetSellPrice());
        Destroy(gameObject);
        return true;
    }

    #endregion



    #region Swap & Move

    protected override void OnSwap(BuyPurchasable<ItemSo> other)
    {
        var otherItem = (BuyItem)other;

        ItemSo tmp = otherItem.Data;
        
        PlayerShopManager.instance.TempItems[Slot.slotIndex] = tmp;
        PlayerShopManager.instance.TempItems[otherItem.Slot.slotIndex] = Data;

        otherItem.ApplyData(Data);
        ApplyData(tmp);      
    }

    #endregion
}