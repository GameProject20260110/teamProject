using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuyItem : BuyThings, IPointerClickHandler, IEndDragHandler
{
    public ItemSo itemInfo;

    public override void OnPointerEnter() { base.OnPointerEnter(); }

    public override void OnPointerExit() { base.OnPointerExit(); }

    public override void OnBeginDrag(PointerEventData eventData) { base.OnBeginDrag(eventData); }

    public override void OnDrag(PointerEventData eventData) { base.OnDrag(eventData); }

    public void UpdateInfo(ItemSo item, bool buy)
    {
        itemInfo = item;
        img.sprite = item.itemIcon;
        Desc.text = itemInfo.itemDesc;
        bought = buy;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (inPotiner)
            {
                DescManager.instance.SelectDesc(childImage.gameObject);
            }

        }
        if (eventData.button == PointerEventData.InputButton.Right && bought)
        {
            DescManager.instance.SellGold(itemInfo.sell);
            Player.instance.PullPlayerItems(itemInfo);
            itemInfo.Reusable();
            Destroy(gameObject);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!bought)
        {
            if (!transform.parent.CompareTag("Inventory") || transform.parent == canvas ||
                GameManager.instance.gold - itemInfo.gold < 0)
            {
                transform.SetParent(previousParent);
                rect.position = previousParent.GetComponent<RectTransform>().position;
            }
            else
            {
                bought = !bought;
                DescManager.instance.BuyGold(itemInfo.gold);
                Player.instance.PushPlayerItems(itemInfo);
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