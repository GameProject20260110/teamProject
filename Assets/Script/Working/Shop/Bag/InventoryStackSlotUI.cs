using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryStackSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;

    public void SetUp(Sprite sprite, int count)
    {
        icon.sprite = sprite;
        bool showCount = count > 1;
        countText.gameObject.SetActive(showCount);
        if (showCount) countText.text = $"x{count}";
    }
}
