using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    [Header("아이템 팝업")]
    [SerializeField] private RectTransform itemPopup;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    private TextMeshProUGUI itemDesc;

    private void Awake()
    {
        if (itemPopup != null)
            itemDesc = itemPopup.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start() => ClosePopup();

    public void OpenPopup(CardData data)
    {
        if (itemPopup == null || data == null) return;

        if (itemDesc != null) itemDesc.text = data.description;
        if (itemIcon != null) itemIcon.sprite = data.diceSkin.GetSprite(1);
        if (itemName != null) itemName.text = data.cardName;

        itemPopup.gameObject.SetActive(true);
    }

    public void ClosePopup()
    {
        if (itemPopup != null) itemPopup.gameObject.SetActive(false);
    }
}