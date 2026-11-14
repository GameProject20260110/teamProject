using UnityEngine;

public class ShopCardItem : ShopItem<CardData>
{
    [SerializeField] private CardView cardView;

    protected override void ApplyData(CardData data)
    {
        Data = data;
        if (data == null) { Hide(); return; }

        cardView.Bind(data);
        if (goldText != null) goldText.text = data.cost.ToString();
    }
}