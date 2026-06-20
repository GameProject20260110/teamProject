using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("카드 UI")]
    public Image rewardIcon;
    public TextMeshProUGUI rewardNameText;
    public TextMeshProUGUI rewardDescText;

    private RewardData _rewardData;
    private BattleItemSo _preSelectedItem;
    private DiceData _preSelectedDice;
    private Action<RewardData, BattleItemSo, DiceData> _onSelected;
    private CardGlow _glow;
    private Vector3 _originalScale;
    private bool _isInteractable = false;

    private const float HOVER_SCALE = 1.15f;
    private const float ANIM_DURATION = 0.3f;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _glow = GetComponentInChildren<CardGlow>();
    }

    public void SetUp(RewardData rewardData, BattleItemSo preSelectedItem, DiceData preSelectedDice, Action<RewardData, BattleItemSo, DiceData> onSelected)
    {
        _rewardData = rewardData;
        _preSelectedItem = preSelectedItem;
        _preSelectedDice = preSelectedDice;
        _onSelected = onSelected;

        if(rewardNameText != null)
            rewardNameText.text = GetRewardName(rewardData);


        if(rewardDescText != null)
            rewardDescText.text = GetRewardDesc(rewardData);


        if (rewardIcon != null)
        {
            Sprite icon = GetRewardIcon(rewardData);
            if (icon != null) rewardIcon.sprite = icon;
        }
            
        Button btn = GetComponent<Button>();
        if(btn != null)
            btn.onClick.AddListener(OnCardClicked);
    }

    public void SetInteractable(bool isInteractable)
    {
        _isInteractable = isInteractable;
    }

    private void OnCardClicked()
    {
        if (!_isInteractable) return;
        _onSelected?.Invoke(_rewardData, _preSelectedItem, _preSelectedDice);
    }

    private string GetRewardName(RewardData data)
    {
        switch (data.rewardType)
        {
            case RewardType.Dice:           return data.dice != null ? data.dice.name : "주사위";
            case RewardType.Gold:           return $"{data.goldAmount} 골드";
            case RewardType.HpPotion:       return $"{data.healAmount} 체력 회복";
            case RewardType.PassiveItem:    
            case RewardType.ActiveItem:     return _preSelectedItem != null ? _preSelectedItem.itemName : "아이템";
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
            case RewardType.PassiveItem:    
            case RewardType.ActiveItem:     return _preSelectedItem != null ? _preSelectedItem.itemDesc : "";
            default: return "";
        }
    }

    private Sprite GetRewardIcon(RewardData data)
    {
        if (data.icon != null) return data.icon;

        switch(data.rewardType)
        {
            case RewardType.Dice:
                return _preSelectedDice?.skin?.GetSprite(1);
            case RewardType.PassiveItem:
            case RewardType.ActiveItem:
                return _preSelectedItem?.itemIcon;
            default: return null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        
        transform.DOKill();
        transform.DOScale(_originalScale * HOVER_SCALE, ANIM_DURATION).SetEase(Ease.OutBack).SetLink(gameObject);
        _glow?.SetGlow(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if( !_isInteractable) return;
        
        transform.DOKill();
        transform.DOScale(_originalScale, ANIM_DURATION).SetEase(Ease.OutQuad).SetLink(gameObject);
        _glow?.SetGlow(false);
    }
}
