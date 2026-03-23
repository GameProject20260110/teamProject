using System.Collections.Generic;
using UnityEngine;

public class CollectionUI : MonoBehaviour
{
    [SerializeField] private DiceDatabase dices;
    [SerializeField] private Transform[] contentParent;
    [SerializeField] private GameObject slotPrefab;

    private List<GameObject> collectionSlots = new();

    public void Open()
    {
        gameObject.SetActive(true);

        ClearSlots();

        foreach(var dice in dices.allDices)
        {
            int parentIndex = (int)dice.type;

            if (parentIndex < 0 || parentIndex >= contentParent.Length) continue;

            var slot = Instantiate(slotPrefab, contentParent[parentIndex]);
            slot.GetComponent<CollectionSlot>().SetData(dice);
            collectionSlots.Add(slot);
        }
    }

    private void ClearSlots()
    {
        foreach(var slot in collectionSlots)
            Destroy(slot);
        collectionSlots.Clear();
    }
}
