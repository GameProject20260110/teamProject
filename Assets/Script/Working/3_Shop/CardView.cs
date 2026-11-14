using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image artImage;          // 카드 일러스트
    [SerializeField] private Image frameImage;        // 등급/타입별 프레임 (선택)
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descText;

    public void Bind(CardData data)
    {
        if (artImage != null) artImage.sprite = data.diceSkin.GetSprite(1);
        if (nameText != null) nameText.text = data.cardName;
        if (costText != null) costText.text = data.cost.ToString();
        if (descText != null) descText.text = data.description;
    }
}
