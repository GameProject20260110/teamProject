using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ItemCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("카드 UI")]
    public Image itemImage;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescText;

    private BattleItemSo _item;
    private Vector3 _originalScale;
    private Vector2 _originalPosition;
    public Vector2 OriginalPosition => _originalPosition;
    private int _originalSiblingIndex;
    private bool _isDragging;

    private const float HOVER_SCALE = 1.3f;
    private const float ANIM_DURATION = 0.5f;
    private const float HOVER_OFFSET_Y = 200f;

    public string GetItemName() => _item != null ? _item.itemName : "";

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void Start()
    {
        _originalPosition = GetComponent<RectTransform>().anchoredPosition;
    }

    public void SetUp(BattleItemSo item)
    {
        if (item == null)
        {
            Debug.LogWarning("ItemCard:SetUp: item이 null입니다.");
            return;
        }
        _item = item;
        if (itemImage != null && item.itemIcon != null)
            itemImage.sprite = item.itemIcon;
        if (itemNameText != null)
            itemNameText.text = item.itemName;
        if (itemDescText != null)
            itemDescText.text = item.itemDesc;
    }

    public void SetDragging(bool isDragging)
    {
        _isDragging = isDragging;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isDragging) return;
        _originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();
        transform.DOScale(_originalScale * HOVER_SCALE, ANIM_DURATION).SetEase(Ease.OutBack);
        GetComponent<RectTransform>()
            .DOAnchorPosY(_originalPosition.y + HOVER_OFFSET_Y, ANIM_DURATION)
            .SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isDragging) return;
        transform.SetSiblingIndex(_originalSiblingIndex);
        transform.DOScale(_originalScale, ANIM_DURATION).SetEase(Ease.OutQuad);
        GetComponent<RectTransform>()
            .DOAnchorPosY(_originalPosition.y, ANIM_DURATION)
            .SetEase(Ease.OutQuad);
    }
    
    public void PlayNegateEffect(GameObject negateOverlayPrefab)
    {
        CanvasGroup cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        DOTween.To(() => cg.alpha, x => cg.alpha = x, 0.5f, 0.3f);
        transform.DOScale(transform.localScale * 0.8f, 0.3f);
        if (negateOverlayPrefab != null)
        {
            GameObject overlay = Instantiate(negateOverlayPrefab, transform);
            overlay.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            overlay.name = "NegateOverlay";
        }
    }

    public void ResetNegateEffect()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null) DOTween.To(() => cg.alpha, x => cg.alpha = x, 1f, 0.3f);
        transform.DOScale(_originalScale, 0.3f);
        Transform overlay = transform.Find("NegateOverlay");
        if (overlay != null) Destroy(overlay.gameObject);
    }

    public void UseItem()
    {
        if (_item == null) return;
        BattleManager.instance.UseItem(_item);
    }
}
