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

    [Header("∞°√≠ µ•¿Ã≈Õ")]
    public DiceGachaTable diceGacha;
    public ItemGachaTable itemGacha;
    
    [Header("±∏∏≈ æ∆¿Ã≈€ ΩΩ∑‘")]
    public ItemSlot[] itemSlots;
    public int DiceSlotNum;
    public int itemSlotNum;

    [Header("«¡∏Æ∆’")]
    public GameObject Dice;
    public GameObject Item;
    public GameObject SpecialSlot;
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
        ReRoll();
        AudioManager.instance.PlayBgm(AudioManager.Bgm.Shop, true);
    }

    public void ReRoll()
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

        for (int i = 0; i < myDicePanel.transform.childCount; i++)
        {
            slotChildDice = myDicePanel.transform.GetChild(i).GetComponentInChildren<BuyDice>();

            if (PlayerManager.instance.dices[i] == null)
            {
                PlayerManager.instance.PushPlayerDices(PlayerManager.instance.defaultDice, i);
            }
            slotChildDice.UpdateDiceInfo(PlayerManager.instance.dices[i], true);

        }

        Transform slotChildItem = null;
        GameObject item;

        for(int i = 0; i < PlayerManager.instance.items.Count; i++)
        {
            slotChildItem = Iventory.transform.GetChild(i);
            item = Instantiate(Item);
            item.GetComponent<BuyItem>().UpdateInfo(PlayerManager.instance.items[i], true);
            item.transform.SetParent(slotChildItem.transform);
            item.transform.GetComponent<RectTransform>().localPosition = Vector3.zero;

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
}
