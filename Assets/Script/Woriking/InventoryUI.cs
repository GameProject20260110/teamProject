using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform totalDeckList;
    [SerializeField] private Transform drawPileList;
    [SerializeField] private GameObject diceIconPrefab;

    private List<GameObject> _totalIcons = new List<GameObject>();
    private List<GameObject> _drawPileIcons = new List<GameObject>();

    private PlayerDeck _playerDeck;
    private DeckManager _deckManager;

    [Inject]
    public void Construct(PlayerDeck playerDeck, DeckManager deckManager)
    {
        _playerDeck = playerDeck;
        _deckManager = deckManager;
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        RefreshList(_playerDeck.inventory, totalDeckList, _totalIcons);
        RefreshList(_deckManager.drawPile, drawPileList, _drawPileIcons);
    }

    private void RefreshList(IList<DiceData> dataList, Transform parent, List<GameObject> icons)
    {
        while (icons.Count < dataList.Count)
        {
            GameObject icon = Instantiate(diceIconPrefab, parent);
            icons.Add(icon);
        }

        for (int i = 0; i < icons.Count; i++)
        {
            if (i < dataList.Count)
            {
                icons[i].SetActive(true);
                icons[i].GetComponent<Image>().sprite = dataList[i].skin.GetSprite(1);
            }
            else
            {
                icons[i].SetActive(false);
            }
        }
    }
}