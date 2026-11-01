using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChestCard : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] private Image PopupBox;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private Image diceSkin;

    public void Setup(Sprite icon, string cardName, string desc)
    {
        textMesh.text = desc;
        diceSkin.sprite = icon;
        this.cardName.text = cardName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PopupBox.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PopupBox.gameObject.SetActive(false);
    }
}
