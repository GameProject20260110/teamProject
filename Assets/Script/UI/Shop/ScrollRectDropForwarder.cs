using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ScrollRect 오브젝트에 추가
public class ScrollRectDropForwarder : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // 드롭된 위치 아래에 있는 모든 UI 오브젝트 탐색
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == gameObject) continue;

            var slot = result.gameObject.GetComponent<ItemSlot>();
            if (slot != null)
            {
                slot.OnDrop(eventData);
                return;
            }
        }
    }
}