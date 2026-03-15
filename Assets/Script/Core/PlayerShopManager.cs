using System.Collections.Generic;
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
    public DiceData ExtraDice;

    [Header("Settings")]
    [SerializeField] private int baseRerollCost = 1;
    public int BaseRerollCost => baseRerollCost;

    public event System.Action<int> OnGoldChanged;
    public event System.Action OnShopCommitted; // 씬 전환용

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void Open()
    {
        var player = PlayerManager.instance;

        TempGold = player.gold;
        RerollCount = 0;

        TempDices = new List<DiceData>(player.dices);
        TempItems = new List<ItemSo>(player.items);
        ExtraDice = player.extraDice;

        IsOpen = true;
        OnGoldChanged?.Invoke(TempGold);
    }

    public void Commit()
    {
        if (!IsOpen)
        {
            Debug.LogWarning("Commit 호출됐지만 상점이 열려있지 않습니다.");
            return;
        }

        var player = PlayerManager.instance;

        player.gold = TempGold;
        player.dices = new List<DiceData>(TempDices);
        player.items = new List<ItemSo>(TempItems);
        player.extraDice = ExtraDice;

        player.Save();

        IsOpen = false;
        OnShopCommitted?.Invoke();
    }

    public void Discard()
    {
        IsOpen = false;
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

        return true;
    }

    public void SellDice(DiceData dice, int slotIndex, int sellPrice)
    {
        if (slotIndex == 6) ExtraDice = PlayerManager.instance.defaultDice;
        else TempDices[slotIndex] = PlayerManager.instance.defaultDice;
        GainGold(sellPrice);
    }

    public void SellItem(ItemSo item, int sellPrice)
    {
        TempItems.Remove(item);
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
