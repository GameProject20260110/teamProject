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

    private void OnValidate()
    {
        if (itemSlots != null && itemSlots.Length != DiceSlotNum + itemSlotNum)
            Debug.LogError($" itemSlots 크기 ({itemSlots.Length}) != DiceSlotNum({DiceSlotNum}) + itemSlotNum({itemSlotNum})(ShopItem)");

        if (Dice == null) Debug.LogWarning("Dice 프리팹이 비어있습니다.(ShopItem)");
        if (Item == null) Debug.LogWarning("Item 프리팹이 비어있습니다.(ShopItem)");
        if (diceGacha == null) Debug.LogWarning("DiceGacha 프리팹이 비어있습니다.(ShopItem)");
        if (itemGacha == null) Debug.LogWarning("ItemGacha 프리팹이 비어있습니다.(ShopItem)");
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
            var diceSlot = myDicePanel.transform.GetChild(i).GetComponentInChildren<ItemSlot>();
            var slotChildDice = diceSlot.GetComponentInChildren<BuyDice>();
            
            diceSlot.SetSpecialSlot(PlayerManager.instance.SpecialSlots[i]);
            
            var dice = PlayerShopManager.instance.TempDices[i] ?? PlayerManager.instance.defaultDice;
            
            slotChildDice.UpdateDiceInfo(dice, true);

        }

        Debug.Log(PlayerShopManager.instance.TempItems.Count);
        for(int i = 0; i < PlayerShopManager.instance.TempItems.Count; i++)
        {
            var slotChildItem = Iventory.transform.GetChild(i);
            var item = Instantiate(Item); 
            item.transform.SetParent(slotChildItem);
            item.GetComponent<RectTransform>().localPosition = Vector3.zero;
            item.GetComponent<BuyItem>().UpdateInfo(PlayerShopManager.instance.TempItems[i], true);

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
