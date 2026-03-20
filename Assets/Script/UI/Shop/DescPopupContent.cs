using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DescPopupContent : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI itemName;
    public Image typeIcon;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI ParchaseGold;
    public TextMeshProUGUI SellGold;
    public TextMeshProUGUI Desc;

    [Header("æ∆¿Ãƒ‹")]
    public Sprite itemTypeSprite;
    public Sprite diceTypeSprite;

    public void UpdataInfo(DiceData data)
    {
        icon.sprite = data.skin.GetSprite(1);
        itemName.text = data.name;
        typeIcon.sprite = diceTypeSprite;
        typeText.text = data.type.ToString();
        ParchaseGold.text = data.gold.ToString();
        SellGold.text = data.sell.ToString();
        Desc.text = data.Desc.ToString();
    }

    public void UpdataInfo(ItemSo data)
    {
        icon.sprite = data.itemIcon;
        itemName.text = data.name;
        typeIcon.sprite = itemTypeSprite;
        typeText.text = "Item";
        ParchaseGold.text = data.gold.ToString();
        SellGold.text = data.sell.ToString();
        Desc.text = data.itemDesc.ToString();
    }
}
