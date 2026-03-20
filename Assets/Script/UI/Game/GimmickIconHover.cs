using UnityEngine;
using UnityEngine.EventSystems;

public class GimmickIconHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GimmickSo gimmick;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (PopupManager.instance == null || gimmick == null) return;
        PopupManager.instance.OpenGimmickPopup(gimmick, GetComponent<RectTransform>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (PopupManager.instance == null) return;
        PopupManager.instance.ClosePopup();
    }
}
