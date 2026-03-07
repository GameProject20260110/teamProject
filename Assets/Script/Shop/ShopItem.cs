using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public static ShopItem instance;

    BuyDice[] buyDice;
    BuyItem[] buyItem;
    public bool hasShoes = false;

    [Header("가챠 데이터")]
    public DiceGachaTable diceGacha;
    public ItemGachaTable itemGacha;
    
    [Header("구매 아이템 슬롯")]
    public ItemSlot[] itemSlots;
    public int DiceSlotNum;
    public int itemSlotNum;

    [Header("프리팹")]
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
        PlayerShopManager.instance.Open();
        SetUp();
        ReRoll();
        AudioManager.instance.PlayBgm(AudioManager.Bgm.Shop, true);
    }

    public void OnLeaveButton()
    {
        PlayerShopManager.instance.Commit();
    }

    public void OnDiscardButton()
    {
        PlayerShopManager.instance.Discard();
    }


    public void ReRoll()
    {
        if (PlayerShopManager.instance.RerollCount > 0)
        {
            bool success = PlayerShopManager.instance.TryReroll();
            if (!success)
            {
                Debug.Log("[Shop] 골드 부족으로 리롤 불가");
                return;
            }
        }
        RerollDice();
        ReRollItem();
    }

    private void SetUp()
    {
        buyDice = new BuyDice[DiceSlotNum];
        buyItem = new BuyItem[itemSlotNum];
        hasShoes = false;

        for (int i = 0; i < myDicePanel.transform.childCount; i++)
        {
            var diceSlot = myDicePanel.transform.GetChild(i).GetComponent<ItemSlot>();
            var slotChildDice = diceSlot.GetComponentInChildren<BuyDice>();
            
            diceSlot.SetSpecialSlot(PlayerManager.instance.SpecialSlots[i]);
            
            var dice = PlayerShopManager.instance.TempDices[i] ?? PlayerManager.instance.defaultDice;
            
            slotChildDice.UpdateDiceInfo(dice, true);

        }

        for(int i = 0; i < PlayerManager.instance.items.Count; i++)
        {
            var slotChildItem = Iventory.transform.GetChild(i);
            var item = Instantiate(Item);
            item.GetComponent<BuyItem>().UpdateInfo(PlayerShopManager.instance.TempItems[i], true);
            item.transform.SetParent(slotChildItem);
            item.GetComponent<RectTransform>().localPosition = Vector3.zero;

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

            buyDice[i].UpdateDiceInfo(diceGacha.Roll(), false);

        }
    }

    private void ReRollItem()
    {
        for(int i = 0; i < itemSlotNum; i++)
        {
            int slotIdx = i + DiceSlotNum;

            if (itemSlots[i + DiceSlotNum].transform.childCount > 0)
                buyItem[i] = itemSlots[i + DiceSlotNum].transform.GetComponentInChildren<BuyItem>();
            else
            {
                buyItem[i] = Instantiate(Item).GetComponent<BuyItem>();
                buyItem[i].transform.SetParent(itemSlots[i + DiceSlotNum].transform);
                buyItem[i].transform.GetComponent<RectTransform>().localPosition = Vector3.zero;
            }
            buyItem[i].UpdateInfo(itemGacha.Roll(), false);
        }

    }
}
