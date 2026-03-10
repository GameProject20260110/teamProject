using TMPro;
using UnityEngine;

public class SlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI goldText;

    public void UpdateSlotUI(string itemName, int gold)
    {
        this.itemName.text = itemName;
        if (goldText == null) return;
        goldText.text = gold.ToString();
    }

}
