using UnityEngine;

public class ShopBattleItem : ShopItem<BattleItemSo>
{
    protected override void ApplyData(BattleItemSo data)
    {
        //Data = data;
        //if (data == null) return;
        //img.sprite = data.itemIcon;
        //if (nameText != null) nameText.text = data.itemName;
        //if (goldText != null) goldText.text = data.gold.ToString();
    }
    //protected override bool OnBuy() => _playerShopManager.TryPurchaseItem(Data);
}
