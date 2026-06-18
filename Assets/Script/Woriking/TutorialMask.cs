using UnityEngine;
using Coffee.UIExtensions;

public class TutorialMask : MonoBehaviour
{
    [SerializeField] private RectTransform unmaskRect;
    [SerializeField] private GameObject screenPanel;
    [SerializeField] private Vector2 padding = new Vector2(40, 40);

    private Unmask unmaskComponent;

    public enum FocusPreset
    {
        None,
        Dice,
        Item
    }

    private void Awake()
    {
        unmaskComponent = unmaskRect.GetComponent<Unmask>();
    }

    public void FocusOnTarget(RectTransform target, FocusPreset preset = FocusPreset.None)
    {
        if (preset != FocusPreset.None)
        {
            ApplyPreset(preset);
            return;
        }

        if(target != null)
        {
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
        
    }

    private void ApplyPreset(FocusPreset preset)
    {
        unmaskRect.anchorMin = new Vector2(0.5f, 0.5f);
        unmaskRect.anchorMax = new Vector2(0.5f, 0.5f);
        unmaskRect.pivot = new Vector2(0.5f, 0.5f);

        switch (preset)
        {
            case FocusPreset.Dice:
                unmaskRect.sizeDelta = new Vector2(1700, 800);
                unmaskRect.anchoredPosition = Vector2.zero;
                break;
            case FocusPreset.Item:
                unmaskRect.sizeDelta = new Vector2(1700, 800);
                unmaskRect.anchoredPosition = new Vector2(880, -400);
                break;

        }
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