using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardVisualView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descText;

    public void SetCard(CardData data)
    {
        if(data == null)
        {
            Clear();
            return;
        }

        if(iconImage != null)
        {
            Sprite sprite = data.diceSkin != null ? data.diceSkin.GetSprite(1) : null;
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
            iconImage.color = Color.white;
        }

        if (nameText != null) nameText.text = data.cardName;
        if (costText != null) costText.text = data.cost.ToString();
        if (descText != null) descText.text = data.description;
    }

    public void Clear()
    {
        if (iconImage != null) iconImage.enabled = false;
        if (nameText != null) nameText.text = "";
        if (costText != null) costText.text = "";
        if (descText != null) descText.text = "";
    }
}
