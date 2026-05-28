using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopDescPopup : MonoBehaviour
{
    public Image icon;
    public Image typeIcon;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI timingText;
    public TextMeshProUGUI Desc;

    [Header("아이콘")]
    public Sprite itemTypeSprite;
    public Sprite diceTypeSprite;

    public void UpdateUI(DiceData data)
    {      
        icon.sprite = data.skin.GetSprite(1);
        typeIcon.sprite = diceTypeSprite;
        typeText.text = data.type.ToString();
        nameText.text = data.abilityName;
        timingText.text = "비어있음";
        Desc.text = data.Desc.ToString();
    }

    public void UpdateUI(ItemSo data)
    {    
        icon.sprite = data.itemIcon;
        typeIcon.sprite = itemTypeSprite;
        nameText.text = data.itemName;
        typeText.text = "Item";
        timingText.text = "비어있음";
        Desc.text = data.itemDesc.ToString();
    }
}
