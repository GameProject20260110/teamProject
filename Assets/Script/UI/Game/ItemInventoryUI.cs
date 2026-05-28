using System.Collections.Generic;
using UnityEngine;

public class ItemInventoryUI : MonoBehaviour
{
    public GameObject itemCardPrefab;
    public RectTransform cardContainer;

    private float _containerWidth;
    private float _cardWidth;
    private float _cardHeight;

    [SerializeField] private float overlapRatio = 0.75f; // 낮을수록 더 겹침
    [SerializeField] private float hiddenOffsetY = -200f; // 숨겨진 위치

    private List<GameObject> _cardObjects = new List<GameObject>();
    private Dictionary<int, ItemCard> _itemIndexToCard = new Dictionary<int, ItemCard>();

    public void Refresh()
    {
        foreach (var card in _cardObjects)
            if (card != null) Destroy(card);

        _cardObjects.Clear();
        _itemIndexToCard.Clear();

        List<BattleItemSo> items = GetItems();
        if (items == null || items.Count == 0) return;

        List<int> validIndices = new List<int>();
        for (int i = 0; i < items.Count; i++)
            if (items[i] != null) validIndices.Add(i);

        if (validIndices.Count == 0) return;

        float frameRatio = 616f / 1035f;
        _containerWidth = cardContainer.rect.width;
        _cardHeight = cardContainer.rect.height * 0.9f;
        _cardWidth = _cardHeight * frameRatio;

        int count = validIndices.Count;

        for (int cardIndex = 0; cardIndex < count; cardIndex++)
        {
            int itemIndex = validIndices[cardIndex];
            GameObject cardObj = Instantiate(itemCardPrefab, cardContainer);
            _cardObjects.Add(cardObj);

            RectTransform rect = cardObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(_cardWidth, _cardHeight);
            rect.anchoredPosition = new Vector2(CalcPosition(cardIndex, count), hiddenOffsetY);

            ItemCard itemCard = cardObj.GetComponent<ItemCard>();
            if (itemCard != null)
            {
                itemCard.SetUp(items[itemIndex]);
                _itemIndexToCard[itemIndex] = itemCard;
            }
        }
    }

    public ItemCard FindCardByName(string itemName)
    {
        foreach (var cardObj in _cardObjects)
        {
            if (cardObj == null) continue;
            ItemCard card = cardObj.GetComponent<ItemCard>();
            if (card != null && card.GetItemName() == itemName) return card;
        }
        return null;
    }

    public ItemCard FindCardByIndex(int index)
    {
        if (_itemIndexToCard.TryGetValue(index, out var card)) return card;
        return null;
    }

    public void ResetCards()
    {
        foreach (var cardObj in _cardObjects)
        {
            if (cardObj == null) continue;
            ItemCard card = cardObj.GetComponent<ItemCard>();
            card?.ResetNegateEffect();
        }
    }

    private float CalcPosition(int index, int total)
    {
        float spacing = _cardWidth * overlapRatio;
        float totalWidth = spacing * (total - 1) + _cardWidth;
        float startX = -totalWidth / 2f + _cardWidth / 2f;
        return startX + index * spacing;
    }

    private List<BattleItemSo> GetItems()
    {
        return ItemManager.instance?.items;
    }
}