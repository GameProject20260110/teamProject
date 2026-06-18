using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform totalDeckList;
    [SerializeField] private Transform drawPileList;
    [SerializeField] private GameObject diceIconPrefab;

    private List<GameObject> _totalIcons = new List<GameObject>();
    private List<GameObject> _drawPileIcons = new List<GameObject>();

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        RefreshList(PlayerDeck.instance.inventory, totalDeckList, _totalIcons);
        RefreshList(DeckManager.instance.drawPile, drawPileList, _drawPileIcons);
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