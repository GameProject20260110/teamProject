using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public static ShopItem instance;

    BuyDice[] buyDice;
    BuyItem[] buyItem;
    public bool hasShoes = false;

    [Header("°¡Ã­ µ¥ÀÌÅÍ")]
    public DiceGachaTable diceGacha;
    public ItemGachaTable itemGacha;
    
    [Header("±¸¸Å ¾ÆÀÌÅÛ ½½·Ô")]
    public ItemSlot[] itemSlots;
    public int DiceSlotNum;
    public int itemSlotNum;

    [Header("ÇÁ¸®ÆÕ")]
    public GameObject Dice;
    public GameObject Item;
    public GameObject myDicePanel;
    public GameObject Iventory;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetUp();
        Reroll();

        AudioManager.instance.PlayBgm(AudioManager.Bgm.Shop, true);
    }



    private void Reroll()
    {
        RerollDice();
        ReRollItem();
    }

    private void SetUp()
    {
        buyDice = new BuyDice[DiceSlotNum];
        buyItem = new BuyItem[itemSlotNum];
        hasShoes = false;

        BuyDice slotChildDice = null;

        for (int i = myDicePanel.transform.childCount - 1; i >= 0; i--)
        {
            slotChildDice = myDicePanel.transform.GetChild(myDicePanel.transform.childCount - 1 - i).GetComponentInChildren<BuyDice>();
            Debug.Log(slotChildDice);
            if (Player.instance.player.DiceSo[i] == null)
            {
                Player.instance.PushPlayerDices(Player.instance.defaultDice,i);
            }
            slotChildDice.UpdateDiceInfo(Player.instance.player.DiceSo[i], true);

        }

        Transform slotChildItem = null;
        GameObject item;

        for (int i = 0; i < Iventory.transform.childCount; i++)
        {
            if (Player.instance.player.itemSo[i] != null)
            {
                slotChildItem = Iventory.transform.GetChild(i);
                item = Instantiate(Item);
                item.GetComponent<BuyItem>().UpdateInfo(Player.instance.player.itemSo[i], true);
                item.transform.SetParent(slotChildItem.transform);
                item.transform.GetComponent<RectTransform>().localPosition = Vector3.zero;
            }
        }
    }

    private void RerollDice()
    {
        for (int i = 0; i < DiceSlotNum; i++)
        {
            if (itemSlots[i].transform.childCount > 0)
            {
                buyDice[i] = itemSlots[i].transform.GetComponentInChildren<BuyDice>();
                     
            }
            else
            {
                buyDice[i] = Instantiate(Dice).GetComponent<BuyDice>();
                buyDice[i].transform.SetParent(itemSlots[i].transform);
                buyDice[i].transform.GetComponent<RectTransform>().localPosition = Vector3.zero;
            }

            DiceData dice = diceGacha.Roll();
            buyDice[i].UpdateDiceInfo(dice, false);

        }
    }

    private void ReRollItem()
    {
        for(int i = 0; i < itemSlotNum; i++)
        {
            if (itemSlots[i + DiceSlotNum].transform.childCount > 0)
            {
                buyItem[i] = itemSlots[i + DiceSlotNum].transform.GetComponentInChildren<BuyItem>();

            }
            else
            {
                buyItem[i] = Instantiate(Item).GetComponent<BuyItem>();
                buyItem[i].transform.SetParent(itemSlots[i + DiceSlotNum].transform);
                buyItem[i].transform.GetComponent<RectTransform>().localPosition = Vector3.zero;
            }

            ItemSo item = itemGacha.Roll();
            buyItem[i].UpdateInfo(item, false);
        }

    }

    public void SelectDiceComb()
    {
        GameObject myDicePanelSlot;
        
        for (int i = 0; i < myDicePanel.transform.childCount; i++)
        {
            myDicePanelSlot = myDicePanel.transform.GetChild(i).gameObject;
            if (myDicePanelSlot.transform.childCount == 0)
            {
                Player.instance.PushPlayerDices(Player.instance.defaultDice,i);

            }
        }
    }

}
