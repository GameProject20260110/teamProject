using UnityEngine;

public class ShopBattleItem : ShopItem<BattleItemSo>
{
    protected override void ApplyData(BattleItemSo data)
    {
        Data = data;
        if (data == null) return;
        img.sprite = data.itemIcon;
    }

    protected override bool OnBuy()
    {
        int cost = Data.gold;
        return PlayerShopManager.instance.TryPurchaseItem(Data);
    }

    protected override void OpenPopup() =>
        PopupManager.instance.OpenPopup(Data, descPosition);

    protected override void OpenDescPopup() =>
        PopupManager.instance.DescOpenPopup(Data);
}
