using System;
using System.Collections.Generic;

public class ShopSession
{
    private readonly ResourceManager _resourceManager;
    private readonly ItemManager _itemManager;
    private readonly PlayerCardCollection _cardCollection;
    private readonly ShopConfig _config;

    private readonly List<CardData> _purchasedCards = new();
    private List<BattleItemSo> _tempItems = new();

    public int Gold { get; private set; }
    public int RerollCount { get; private set; }
    public int RerollCost => _config.baseRerollCost + RerollCount;
    public bool IsOpen { get; private set; }

    public IReadOnlyList<CardData> PurchasedCards => _purchasedCards;
    public IReadOnlyList<BattleItemSo> TempItems => _tempItems;

    public event Action<int> GoldChanged;

    public ShopSession(ResourceManager resourceManager, ItemManager itemManager,
                       PlayerCardCollection cardCollection, ShopConfig config)
    {
        _resourceManager = resourceManager;
        _itemManager = itemManager;
        _cardCollection = cardCollection;
        _config = config;
    }

    public void Open()
    {
        Gold = _resourceManager.gold;
        RerollCount = 0;
        _purchasedCards.Clear();
        _tempItems = new List<BattleItemSo>(_itemManager.items);
        IsOpen = true;
        GoldChanged?.Invoke(Gold);
    }

    public bool TryPurchaseCard(CardData card)
    {
        if (!IsOpen || card == null || !TrySpend(card.cost)) return false;
        _purchasedCards.Add(card);
        return true;
    }

    public bool TryPurchaseItem(BattleItemSo item)
    {
        if (!IsOpen || item == null || !TrySpend(item.gold)) return false;
        _tempItems.Add(item);
        return true;
    }

    public bool TryReroll()
    {
        if (!IsOpen || !TrySpend(RerollCost)) return false;
        RerollCount++;
        return true;
    }

    public void Commit()
    {
        if (!IsOpen) return;

        _resourceManager.gold = Gold;
        _itemManager.items = new List<BattleItemSo>(_tempItems);
        foreach (var card in _purchasedCards)
            _cardCollection.AddCard(card.cardID);

        _resourceManager.Save();
        _itemManager.Save();
        IsOpen = false;
    }

    public void Discard() => IsOpen = false;

    private bool TrySpend(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        GoldChanged?.Invoke(Gold);
        return true;
    }
}