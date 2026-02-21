using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuyDice : BuyThings, IPointerClickHandler, IEndDragHandler
{
    public DiceData DiceInfo;
    public RectTransform DescPosition;
    public int index;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void OnPointerEnter(PointerEventData eventData) { 
        base.OnPointerEnter(eventData);
        Debug.Log("enter");
        PopupManager.instance.OpenPopup(DiceInfo, DescPosition);
    }

    public override void OnPointerExit(PointerEventData eventData) { 
        base.OnPointerExit(eventData);
        Debug.Log("exit");
        PopupManager.instance.ClosePopup();
    }

    public override void OnBeginDrag(PointerEventData eventData) { base.OnBeginDrag(eventData); }

    public override void OnDrag(PointerEventData eventData) { base.OnDrag(eventData); }

    public void UpdateDiceInfo(DiceData data, bool buy)
    {
        DiceInfo = data;
        img.sprite = data.skin.GetSprite(1);
        bought = buy;
        index = GetComponentInParent<ItemSlot>().slotIndex;
        DescPosition = GetComponentsInChildren<RectTransform>(true)[1];
    }

    public void ChangeDiceInfo(DiceData data)
    {
        DiceInfo = data;
        img.sprite = data.skin.GetSprite(1);
        PlayerManager.instance.PushPlayerDices(data, index);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
        }
        if (eventData.button == PointerEventData.InputButton.Right && bought)
        {
            Debug.Log(index);
            if(ShopItem.instance.hasShoes) 
                DescManager.instance.SellGold(DiceInfo.gold);
            else
                DescManager.instance.SellGold(DiceInfo.sell);
            PlayerManager.instance.PullPlayerDices(DiceInfo, index);
            Destroy(gameObject);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GameObject otherObject = eventData.pointerCurrentRaycast.gameObject;
        if (!bought)
        {
            if (!transform.parent.CompareTag("MySlot") || transform.parent == canvas ||
                PlayerManager.instance.gold - DiceInfo.gold < 0)
            {
                transform.SetParent(previousParent);
                rect.position = previousParent.GetComponent<RectTransform>().position;
            }
            else
            {
                index = GetComponentInParent<ItemSlot>().slotIndex;
                bought = !bought;
                DescManager.instance.BuyGold(DiceInfo.gold);
                PlayerManager.instance.PushPlayerDices(DiceInfo, index);
            }
        }
        else
        {
            if (otherObject == null)
            {
                transform.SetParent(previousParent);
                rect.position = previousParent.GetComponent<RectTransform>().position;
            }
            else if (otherObject.CompareTag("BuyDice"))
            {
                Debug.Log("Change");
                DiceData tempDiceInfo = otherObject.GetComponent<BuyDice>().DiceInfo;
                otherObject.GetComponent<BuyDice>().ChangeDiceInfo(DiceInfo);
                ChangeDiceInfo(tempDiceInfo);
                transform.SetParent(previousParent);
                rect.position = previousParent.GetComponent<RectTransform>().position;
            }
            else if (transform.parent == canvas || !transform.parent.CompareTag("MySlot"))
            {
                transform.SetParent(previousParent);
                rect.position = previousParent.GetComponent<RectTransform>().position;
            }
            else
            {
                PlayerManager.instance.PullPlayerDices(DiceInfo, index);
                index = GetComponentInParent<ItemSlot>().slotIndex;
                PlayerManager.instance.PushPlayerDices(DiceInfo, index);
            }
            
        }

        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;
        isDragged = false;
    }

}

