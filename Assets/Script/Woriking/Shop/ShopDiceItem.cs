using UnityEngine;

public class ShopDiceItem : ShopItem<DiceData>
{
    protected override void ApplyData(DiceData data)
    {
        Data = data;
        if (data == null) return;
        img.sprite = data.skin.GetSprite(1);
    }

    protected override bool OnBuy()
    {
        int cost = LuckyStone.CalcDiscount(Data.gold);
        return PlayerShopManager.instance.TryPurchaseDice(Data);
    }

    protected override void OpenPopup() =>
        PopupManager.instance.OpenPopup(Data, descPosition);

    protected override void OpenDescPopup() =>
        PopupManager.instance.DescOpenPopup(Data);

    public int GetTier() => Data.tier;
}