using UnityEngine;
using UnityEngine.EventSystems;

public class GimmickIconHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GimmickSo gimmick;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GamePopupManager.instance == null || gimmick == null) return;
        GamePopupManager.instance.OpenGimmickPopup(gimmick, GetComponent<RectTransform>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GamePopupManager.instance == null) return;
        GamePopupManager.instance.ClosePopup();
    }
}
