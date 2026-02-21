using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuyItem : BuyThings, IPointerClickHandler, IEndDragHandler
{
    public ItemSo itemInfo;
    public RectTransform DescPosition;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        PopupManager.instance.OpenPopup(itemInfo, DescPosition);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        PopupManager.instance.ClosePopup();
    }

    public override void OnBeginDrag(PointerEventData eventData) { base.OnBeginDrag(eventData); }

    public override void OnDrag(PointerEventData eventData) { base.OnDrag(eventData); }

    public void UpdateInfo(ItemSo item, bool buy)
    {
        itemInfo = item;
        img.sprite = item.itemIcon;
        bought = buy;
        DescPosition = GetComponentsInChildren<RectTransform>(true)[1];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
        }
        if (eventData.button == PointerEventData.InputButton.Right && bought)
        {
            DescManager.instance.SellGold(itemInfo.sell);
            PlayerManager.instance.PullPlayerItems(itemInfo);
            itemInfo.Reusable();
            Destroy(gameObject);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!bought)
        {
            if (!transform.parent.CompareTag("Inventory") || transform.parent == canvas ||
                PlayerManager.instance.gold - itemInfo.gold < 0)
            {
                transform.SetParent(previousParent);
                rect.position = previousParent.GetComponent<RectTransform>().position;
            }
            else
            {
                bought = !bought;
                DescManager.instance.BuyGold(itemInfo.gold);
                PlayerManager.instance.PushPlayerItems(itemInfo);
                itemInfo.Consumable();
                itemInfo.Reusable();
                
                if(itemInfo.isConsumable) Destroy(gameObject);
            }
        }
        else
        {
            if (transform.parent == canvas || !transform.parent.CompareTag("Inventory"))
            {
                transform.SetParent(previousParent);
                rect.position = previousParent.GetComponent<RectTransform>().position;
            }
        }

        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;
        isDragged = false;
    }

}