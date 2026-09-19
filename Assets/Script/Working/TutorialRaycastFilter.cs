using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;

public class TutorialRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    [SerializeField] private Unmask targetUnmask;

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (targetUnmask == null || !targetUnmask.isActiveAndEnabled)
            return true;

        RectTransform unmaskRect = targetUnmask.transform as RectTransform;

        bool isInside;
        if (eventCamera != null)
        {
            isInside = RectTransformUtility.RectangleContainsScreenPoint(unmaskRect, screenPoint, eventCamera);
        }
        else
        {
            isInside = RectTransformUtility.RectangleContainsScreenPoint(unmaskRect, screenPoint);
        }

        // 반전! Unmask 영역 밖만 Screen이 클릭 받음
        return !isInside;
    }
}
