using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerShopManager : MonoBehaviour
{
    public static PlayerShopManager instance;

    private const int EXTRA_DICE_SLOT_INDEX = 6;

    public int TempGold { get; private set; }
    public int RerollCount { get; private set; }
    public int RerollCost => BaseRerollCost + RerollCount;

    public List<DiceData> TempDices = new();
    public List<ItemSo> TempItems = new();
    public List<ItemSo> pendingConsumables = new List<ItemSo>();
    public DiceData ExtraDice;

    [Header("Settings")]
    [SerializeField] private int baseRerollCost = 1;
    public int BaseRerollCost => baseRerollCost;
    public int maxShopCount = 3;
    public int currentShopCount = 3;

    public bool ClearRound = false;

    [Header("UI References")]
    [SerializeField] private GameObject shopCanvas;
    [SerializeField] private RectTransform shopPanel;
    [SerializeField] private ShopUIController shopUIController;
    [SerializeField] private TextMeshProUGUI shopCountText;
    [SerializeField] private ShopPanelAnimator shopAnimator;

    public event System.Action<int> OnGoldChanged;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void Open()
    {        
        
        var player = PlayerManager.instance;

        TempGold = player.gold;
        RerollCount = 0;

        currentShopCount = PlayerManager.instance.ShopCount;
        currentShopCount--;
        shopCountText.text = $"{currentShopCount} / {maxShopCount}";

        TempDices = new List<DiceData>(player.dices);
        TempItems = new List<ItemSo>(player.items);
        ExtraDice = player.extraDice;

        IsOpen = true;
        OnGoldChanged?.Invoke(TempGold);

    }

    public async void OpenWithAnimation()
    {
        if (PlayerManager.instance.ShopCount <= 0)
        {
            Debug.Log("상점을 열 수 없습니다.");
            return;
        }

        Open();
        shopCanvas.SetActive(true);
        if (shopAnimator != null && shopAnimator.gameObject != null)
        {
            await shopAnimator.Show();
        }
        else
        {
            Debug.LogError("shopAnimator 또는 gameObject가 null입니다!");
        }
    }

    public void Commit()
    {
        if (!IsOpen)
        {
            Debug.LogWarning("Commit 호출됐지만 상점이 열려있지 않습니다.");
            return;
        }

        var player = PlayerManager.instance;

        player.ShopCount = currentShopCount;

        player.gold = TempGold;
        player.dices = new List<DiceData>(TempDices);
        player.items = new List<ItemSo>(TempItems);
        player.extraDice = ExtraDice;

        player.Save();
        IsOpen = false;
    }

    public async void CommitWithAnimation()
    {
        Commit();
        await shopAnimator.Hide();
    }

    public void Discard()
    {
        IsOpen = false;
        shopAnimator.Hide().Forget();
        Debug.Log("상점 변경사항 폐기");
    }


    //--- 구매 / 판매 / 리롤 ---

    public bool TryPurchaseDice(DiceData dice, int slotIndex)
    {
        int cost = LuckyStone.CalcDiscount(dice.gold);
        if (!HasEnoughGold(cost)) return false;

        SpendGold(cost);

        if(slotIndex == 6) ExtraDice = dice;
        else TempDices[slotIndex] = dice;

        return true;
    }

    public bool TryPurchaseItem(ItemSo item, int slotIndex)
    {
        int cost = LuckyStone.CalcDiscount(item.gold);
        if (!HasEnoughGold(cost)) return false;

        SpendGold(cost);
        TempItems[slotIndex] = item;
        return true;
    }

    public bool TryPurchaseSpecialSlot(int cost, int slotIndex)
    {
        if (!HasEnoughGold(cost)) return false;

        SpendGold(cost);
        PlayerManager.instance.SpecialSlots[slotIndex] = true;
        PlayerManager.instance.ShopLevel++;

        return true;
    }

    public void SellDice(DiceData dice, int slotIndex, int sellPrice)
    {
        if (slotIndex == 6) ExtraDice = PlayerManager.instance.defaultDice;
        else TempDices[slotIndex] = PlayerManager.instance.defaultDice;
        GainGold(sellPrice);
    }

    public void SellItem(ItemSo item, int slotIndex, int sellPrice)
    {
        TempItems[slotIndex] = null;
        GainGold(sellPrice);
    }

    public bool TryReroll()
    {
        if (!HasEnoughGold(RerollCost)) return false;

        SpendGold(RerollCost);
        RerollCount++;
        return true;
    }

    public void SetDiceAtSlot(int slotIndex, DiceData data)
    {
        if (slotIndex == EXTRA_DICE_SLOT_INDEX)
            ExtraDice = data;
        else 
            TempDices[slotIndex] = data;
    }

    //---------- Private -------------

    private bool HasEnoughGold(int amount) => TempGold >= amount;

    private void SpendGold(int amount)
    {
        TempGold -= amount;
        OnGoldChanged?.Invoke(TempGold);
    }

    private void GainGold(int amount)
    {
        TempGold += amount;
        OnGoldChanged?.Invoke(TempGold);
    }
}
