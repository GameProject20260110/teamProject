using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BagInventroyUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject panelRoot;

    [Header("영역별 컨테이너")]
    [SerializeField] private Transform diceContainer;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject stackSlotPrefab;

    private readonly List<GameObject> _spawnedDiceSlots = new();
    private readonly List<GameObject> _spawnedItemSlots = new();

    public void Toggle()
    {
        bool next = !panelRoot.activeSelf;
        panelRoot.SetActive(next);
        if (next) Refresh();
    }

    public void Refresh()
    {
        ClearSlots(_spawnedDiceSlots);
        ClearSlots(_spawnedItemSlots);

        BuildDiceSlots(PlayerDeck.instance.inventory);
        BuildItemSlots(ItemManager.instance.items);
    }

    private void BuildDiceSlots(List<DiceData> dices)
    {
        var grouped = dices.Where(d => d != null).GroupBy(d => d);
        foreach (var group in grouped)
        {
            var slotObj = Instantiate(stackSlotPrefab, diceContainer);
            slotObj.GetComponent<InventoryStackSlotUI>().SetUp(group.Key.skin.GetSprite(1), group.Count());
            _spawnedDiceSlots.Add(slotObj);
        }
    }

    private void BuildItemSlots(List<BattleItemSo> itmes)
    {
        var grouped = itmes.Where(i => i != null).GroupBy(i => i);
        foreach(var group in grouped)
        {
            var slotObj = Instantiate(stackSlotPrefab, itemContainer);
            slotObj.GetComponent<InventoryStackSlotUI>().SetUp(group.Key.itemIcon, group.Count());
            _spawnedItemSlots.Add(slotObj);
        }
    }

    private void ClearSlots(List<GameObject> slots)
    {
        foreach(var go in slots)
        {
            if (go != null) Destroy(go);
            slots.Clear();
        }
    }
}
