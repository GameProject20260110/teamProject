using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardCardUI : MonoBehaviour
{
    [Header("카드 UI")]
    public Image rewardIcon;
    public TextMeshProUGUI rewardNameText;
    public TextMeshProUGUI rewardDescText;

    private RewardData _rewardData;
    private Action<RewardData> _onSelected;
    private CardGlow _glow;
    private Vector3 _originalScale;
    private Vector2 _originalPosition;
    private int _originalSiblingIndex;

    private const float HOVER_SCALE = 1.15f;
    private const float ANIM_DURATION = 0.5f;
    private const float HOVER_OFFSET_Y = 30f;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _glow = GetComponentInChildren<CardGlow>();
    }

    private void Start()
    {
        _originalPosition = GetComponent<RectTransform>().anchoredPosition;

    }

    public void SetUp(RewardData rewardData, Action<RewardData> onSelected)
    {
        _rewardData = rewardData;
        _onSelected = onSelected;

        if(rewardNameText != null)
            rewardNameText.text = GetRewardName(rewardData);


        if(rewardDescText != null)
            rewardDescText.text = GetRewardDesc(rewardData);


        if (rewardIcon != null || rewardData.icon != null)
            rewardIcon.sprite = GetRewardIcon(rewardData);

        Button btn = GetComponent<Button>();
        if(btn != null)
            btn.onClick.AddListener(OnCardClicked);
    }

    private void OnCardClicked()
    {
        _onSelected?.Invoke(_rewardData);
    }

    private string GetRewardName(RewardData data)
    {
        switch (data.rewardType)
        {
            case RewardType.Dice:           return data.dice != null ? data.dice.name : "주사위";
            case RewardType.Gold:           return $"{data.goldAmount} 골드";
            case RewardType.HpPotion:       return $"{data.healAmount} 체력 회복";
            case RewardType.PassiveItem:    return data.item != null ? data.item.itemName : "아이템";
            case RewardType.ActiveItem:     return data.item != null ? data.item.itemName : "아이템";
            default: return "";
        }
    }

    private string GetRewardDesc(RewardData data)
    {
        switch (data.rewardType)
        {
            case RewardType.Dice:           return data.dice != null ? data.dice.name : "";
            case RewardType.Gold:           return $"골드 {data.goldAmount}개를 획득합니다.";
            case RewardType.HpPotion:       return $"체력을 {data.healAmount}만큼 회복합니다.";
            case RewardType.PassiveItem:    return data.item != null ? data.item.itemDesc : "";
            case RewardType.ActiveItem:     return data.item != null ? data.item.itemDesc : "";
            default: return "";
        }
    }

    private Sprite GetRewardIcon(RewardData data)
    {
        if (data.icon != null) return data.icon;

        switch(data.rewardType)
        {
            case RewardType.PassiveItem:
            case RewardType.ActiveItem:
                return data.item?.itemIcon;
            default: return null;
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        
        _originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();
        transform.DOScale(_originalScale * HOVER_SCALE, ANIM_DURATION).SetEase(Ease.OutBack);
        GetComponent<RectTransform>()
            .DOAnchorPosY(_originalPosition.y + HOVER_OFFSET_Y, ANIM_DURATION)
            .SetEase(Ease.OutBack);
        _glow?.SetGlow(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.SetSiblingIndex(_originalSiblingIndex);
        transform.DOScale(_originalScale, ANIM_DURATION).SetEase(Ease.OutQuad);
        GetComponent<RectTransform>()
            .DOAnchorPosY(_originalPosition.y, ANIM_DURATION)
            .SetEase(Ease.OutQuad);
        _glow?.SetGlow(false);
    }
}
