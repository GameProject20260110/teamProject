using UnityEngine;
using TMPro;

public class GameDiceTooltipController : MonoBehaviour
{
    public static GameDiceTooltipController instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private float verticalOffset = 100f;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        Hide();
    }

    public void Show(string title, string desc, Transform target)
    {
        panel.SetActive(true);
        titleText.text = title;
        descText.text = desc;

        SetPosition(target);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private void SetPosition(Transform target)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, target.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelRect.parent as RectTransform,
            screenPoint,
            uiCamera,
            out Vector2 localPos
        );

        localPos.y -= verticalOffset;
        panelRect.anchoredPosition = localPos;
    }
}