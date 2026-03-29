using UnityEngine;
using Coffee.UIExtensions;

public class TutorialMask : MonoBehaviour
{
    [SerializeField] private RectTransform unmaskRect;
    [SerializeField] private GameObject screenPanel;
    [SerializeField] private Vector2 padding = new Vector2(40, 40);

    private Unmask unmaskComponent;

    private void Awake()
    {
        unmaskComponent = unmaskRect.GetComponent<Unmask>();
    }

    public void FocusOnTarget(RectTransform target)
    {
        if (target == null)
        {
            Debug.LogError("target 이 없음");
            return;
        }

        // 1차 위치랑 크기 맞추기
        unmaskComponent.FitTo(target);
        Vector2 newSize = target.rect.size + padding;
        unmaskRect.sizeDelta = newSize;

        // pivot에 따른 위치 보정
        Vector2 pivotOffset = new Vector2(
            (target.pivot.x - 0.5f) * padding.x,
            (target.pivot.y - 0.5f) * padding.y
        );
        unmaskRect.anchoredPosition += pivotOffset;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}