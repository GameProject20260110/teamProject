using System.Collections.Generic;
using UnityEngine;

public class ItemInventoryUI : MonoBehaviour
{
    public GameObject itemCardPrefab;
    public RectTransform cardContainer;

    public float containerWidth;
    public float cardWidth;
    public float cardHeight;
    public int maxSpreadCount = 4;

    private List<GameObject> _cardObject = new List<GameObject>();

    public void Refresh()
    {
        foreach(var card in _cardObject)
        {
            if (card != null) Destroy(card);
        }
        _cardObject.Clear();

        float frameRatio = 616f / 1035f;
        containerWidth = cardContainer.rect.width;
        cardHeight = cardContainer.rect.height * 0.9f;
        cardWidth = cardHeight * frameRatio;

        List<ItemSo> items = GetItems();
        if (items == null || items.Count == 0) return;

        int count = items.Count;
        bool isOverLap = count > maxSpreadCount;

        for(int i = 0; i < count; i++)
        {
            GameObject cardObj = Instantiate(itemCardPrefab, cardContainer);
            _cardObject.Add(cardObj);

            RectTransform rect = cardObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(cardWidth, cardHeight);
            rect.anchoredPosition = new Vector2(CalcPosition(i, count, isOverLap), 0);

            ItemCard itemCard = cardObj.GetComponent<ItemCard>();
            if (itemCard != null) itemCard.SetUp(items[i]);
        }
    }

    private float CalcPosition(int index, int total, bool isOverlap)
    {
        if(!isOverlap)
        {
            float spacing = (containerWidth - cardWidth * total) / (total + 1);
            float startX = -(containerWidth / 2) + spacing + cardWidth / 2;
            return startX + index * (cardWidth + spacing);
        }
        else
        {
            float overlapSpacing = (containerWidth - cardWidth) / (total - 1);
            float startX = -(containerWidth / 2) + cardWidth / 2;
            return startX + index * overlapSpacing;
        }
    }

    private List<ItemSo> GetItems()
    {
        if(TestModeManager.instance != null && TestModeManager.instance.isTestModeActive)
        {
            return TestModeManager.instance.testItem;
        }
        return PlayerManager.instance?.items;
    }
}
