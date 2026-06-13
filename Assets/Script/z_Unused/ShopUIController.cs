//using UnityEngine;
//using UnityEngine.UI;

//public class ShopUIController : MonoBehaviour
//{
//    public static ShopUIController instance;

//    [Header("Shop Items")]
//    private BuyDice[] buyDice;
//    private BuyItem[] buyItem;

//    [Header("Gacha Tables")]
//    [SerializeField] private DiceGachaDatabase ShopDiceDatabase;
//    [SerializeField] private DiceGachaTable diceGacha;
//    [SerializeField] private ItemGachaTable itemGacha;
    
//    [Header("Slot Settings")]
//    [SerializeField] private ItemSlot[] itemSlots;
//    [SerializeField] private int diceSlotCount = 6;
//    [SerializeField] private int itemSlotCount = 3;

//    [Header("Prefabs")]
//    public GameObject dicePrefab;
//    public GameObject itemPrefab;

//    [Header("Panel")]
//    public GameObject myDicePanel;
//    public GameObject iventoryPanel;
//    public GameObject extraDiceSlot;
//    public NotificationUI notificationUI;

//    [Header("Sprite")]
//    [SerializeField] private Sprite[] tierSlotImages = new Sprite[5];

//    private void Awake()
//    {
//        if(instance == null) instance = this;
//        else Destroy(gameObject);

//    }

//    public void Initialize()
//    {
//        InitializeShop();
//        ReRoll();        

//        if (notificationUI != null) notificationUI.gameObject.SetActive(false);
//    }

//    #region Shop Managment

//    public void OnLeaveButton()
//    {
//        PlayerShopManager.instance.Commit();
//    }

//    public void OnDiscardButton()
//    {
//        PlayerShopManager.instance.Discard();
//    }


//    public void ReRoll()
//    {
//        if (PlayerShopManager.instance.RerollCount > 0)
//        {
//            bool success = PlayerShopManager.instance.TryReroll();
//            if (!success) return;
//        }

//        RerollDice();

//        ReRollItem();
//    }

//    #endregion



//    #region Initialization

//    private void InitializeShop()
//    {
//        buyDice = new BuyDice[diceSlotCount];
//        buyItem = new BuyItem[itemSlotCount];

//        //InitializePlayerDices();
//        //InitializePlayerInventory();
//    }

//    private void InitializePlayerDices()
//    {
//        int childCount = myDicePanel.transform.childCount;

//        for (int i = 0; i < childCount; i++)
//        {
//            var diceSlot = myDicePanel.transform.GetChild(i).GetComponentInChildren<ItemSlot>();

//            var slotChildDice = diceSlot.GetComponentInChildren<BuyDice>();
            
//            var dice = PlayerShopManager.instance.TempDices[i] 
//                ?? PlayerManager.instance.defaultDice;
            
//            slotChildDice.UpdateDiceInfo(dice, true);

//        }
//    }

//    private void InitializePlayerInventory()
//    {
//        var tempItems = PlayerShopManager.instance.TempItems;

//        for (int i = 0; i < tempItems.Count; i++)
//        {
//            var slotChildItem = iventoryPanel.transform.GetChild(i).GetComponentInChildren<BuyItem>(true);
//            var item = tempItems[i];

//            slotChildItem.gameObject.SetActive(true);
//            if(item != null)
//            {               
//                slotChildItem.UpdateInfo(item, true);
//            }
//            else
//            {
//                slotChildItem.UpdateInfo(item, true);
//                slotChildItem.gameObject.SetActive(false);
//            }
//        }
//    }

//    #endregion



//    #region Reroll

//    private void RerollDice()
//    {
//        for (int i = 0; i < diceSlotCount; i++)
//        {
            
//            if (itemSlots[i].transform.childCount > 0)
//            {
//                buyDice[i] = itemSlots[i].transform.GetComponentInChildren<BuyDice>();                  
//            }
//            else
//            {
//                buyDice[i] = Instantiate(dicePrefab).GetComponent<BuyDice>();
//                buyDice[i].transform.SetParent(itemSlots[i].transform);
//                buyDice[i].transform.GetComponent<RectTransform>().localPosition = Vector3.zero;
//            }
//            var dicega = ShopDiceDatabase.diceGachaList[0];
//            buyDice[i].UpdateDiceInfo(dicega.Roll(), false);
//            itemSlots[i].GetComponent<Image>().sprite = tierSlotImages[buyDice[i].GetTier() - 1];

//        }
//    }

//    private void ReRollItem()
//    {
//        for(int i = 0; i < itemSlotCount; i++)
//        {
//            if (itemSlots[i + diceSlotCount].transform.childCount > 0)
//                buyItem[i] = itemSlots[i + diceSlotCount].transform.GetComponentInChildren<BuyItem>();
//            else
//            {
//                buyItem[i] = Instantiate(itemPrefab).GetComponent<BuyItem>();
//                buyItem[i].transform.SetParent(itemSlots[i + diceSlotCount].transform);
//                buyItem[i].transform.GetComponent<RectTransform>().localPosition = Vector3.zero;
//            }

//            buyItem[i].GetComponentsInChildren<RectTransform>()[1].localPosition = new Vector2(0, 200);
//            buyItem[i].UpdateInfo(itemGacha.Roll(), false);
//            itemSlots[i + diceSlotCount].GetComponent<Image>().sprite = tierSlotImages[buyItem[i].GetTier() - 1];
//        }

//    }

//    #endregion



//    #region Validation

//    private void OnValidate()
//    {
//        if (itemSlots != null && itemSlots.Length != diceSlotCount + itemSlotCount)
//            Debug.LogError($" itemSlots 크기 ({itemSlots.Length}) != DiceSlotNum({diceSlotCount}) + itemSlotNum({itemSlotCount})(ShopItem)");

//        if (dicePrefab == null) Debug.LogWarning("Dice 프리팹이 비어있습니다.(ShopItem)");
//        if (itemPrefab == null) Debug.LogWarning("Item 프리팹이 비어있습니다.(ShopItem)");
//        if (diceGacha == null) Debug.LogWarning("DiceGacha 프리팹이 비어있습니다.(ShopItem)");
//        if (itemGacha == null) Debug.LogWarning("ItemGacha 프리팹이 비어있습니다.(ShopItem)");
//    }

//    #endregion



//    #region Tutorial

//    private void SetUpTutorial()
//    {
//        for (int i = 0; i < diceSlotCount; i++)
//        {
//            buyDice[i] = itemSlots[i].transform.GetComponentInChildren<BuyDice>();
//            buyDice[i].UpdateDiceInfo(diceGacha.diceWeights[24].dice, false);
//            itemSlots[i].GetComponent<Image>().sprite = tierSlotImages[buyDice[i].GetTier() - 1];
//        }

//        for (int i = 0; i < itemSlotCount; i++)
//        {
//            buyItem[i] = itemSlots[i + diceSlotCount].transform.GetComponentInChildren<BuyItem>();
//            buyItem[i].GetComponentsInChildren<RectTransform>()[1].localPosition = new Vector2(0, 200);
//            buyItem[i].UpdateInfo(itemGacha.items[2]._item, false);
//            itemSlots[i + diceSlotCount].GetComponent<Image>().sprite = tierSlotImages[buyItem[i].GetTier() - 1];
//        }
//    }

//    #endregion
//}
