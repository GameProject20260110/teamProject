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

    [Header("æ∆¿Ãƒ‹")]
    public Sprite itemTypeSprite;
    public Sprite diceTypeSprite;

    public void UpdateUI(DiceData data)
    {      
        icon.sprite = data.skin.GetSprite(1);
        typeIcon.sprite = diceTypeSprite;
        typeText.text = data.type.ToString();
        nameText.text = data.abilityName;
        timingText.text = data.timing.ToString();
        Desc.text = data.Desc.ToString();
    }

    public void UpdateUI(ItemSo data)
    {    
        icon.sprite = data.itemIcon;
        typeIcon.sprite = itemTypeSprite;
        nameText.text = data.itemName;
        typeText.text = "Item";
        timingText.text = data.timing.ToString();
        Desc.text = data.itemDesc.ToString();
    }
}
