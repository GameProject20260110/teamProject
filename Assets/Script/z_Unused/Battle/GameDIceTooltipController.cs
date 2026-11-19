using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameDiceTooltipController : MonoBehaviour
{
    public static GameDiceTooltipController instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private Image tooltipImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private RectTransform panelRect;
    private Camera uiCamera;
    [SerializeField] private float verticalOffset = 100f;

    private const float BoardCenterY = 0f;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        uiCamera = Camera.main;
        Hide();
    }

    public void Show(string title, string desc, Sprite diceSkin, Transform target)
    {
        panel.SetActive(true);
        tooltipImage.sprite = diceSkin;
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

        bool isMySide = target.position.y < BoardCenterY;
        localPos.y += isMySide ? verticalOffset : -verticalOffset;

        ClampToParentBounds(ref localPos);

        panelRect.anchoredPosition = localPos;
    }

    private void ClampToParentBounds(ref Vector2 localPos)
    {
        var parentRect = panelRect.parent as RectTransform;
        if (parentRect == null) return;

        float halfWidth = panelRect.rect.width * 0.5f;
        float halfHeight = panelRect.rect.height * 0.5f;
        float parentHalfWidth = parentRect.rect.width * 0.5f;
        float parentHalfHeight = parentRect.rect.height * 0.5f;

        localPos.x = Mathf.Clamp(localPos.x, -parentHalfWidth + halfWidth, parentHalfWidth - halfWidth);
        localPos.y = Mathf.Clamp(localPos.y, -parentHalfHeight + halfHeight, parentHalfHeight - halfHeight);
    }
}