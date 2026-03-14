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

    private ItemSo _item;
    private Vector3 _originalScale;
    private int _originalSiblingIndex;

    private const float HOVER_SCALE = 1.5f;
    private const float ANIM_DURATION = 0.5f;

    public void SetUp(ItemSo item)
    {
        if(item == null)
        {
            Debug.LogWarning("ItemCard:SetUp: item이 null입니다.");
            return;
        }

        _item = item;

        if(itemImage != null && item.itemIcon != null)
        {
            itemImage.sprite = item.itemIcon;
        }

        if(itemNameText != null)
        {
            itemNameText.text = item.itemName;
        }
        if(itemDescText != null)
        {
            itemDescText.text = item.itemDesc;
        }
    }

    private void Start()
    {
        _originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();
        transform.DOScale(_originalScale * HOVER_SCALE, ANIM_DURATION).SetEase(Ease.OutBack);

        RectTransform rect = GetComponent<RectTransform>();
        rect.DOAnchorPosY(rect.anchoredPosition.y + rect.rect.height * 0.25f, ANIM_DURATION).SetEase(Ease.OutBack); 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.SetSiblingIndex(_originalSiblingIndex);
        transform.DOScale(_originalScale, ANIM_DURATION).SetEase(Ease.OutQuad);
        RectTransform rect = GetComponent<RectTransform>();
        rect.DOAnchorPosY(0, ANIM_DURATION).SetEase(Ease.OutQuad);
    }
}
