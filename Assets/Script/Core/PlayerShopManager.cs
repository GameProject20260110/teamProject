using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShopManager : MonoBehaviour
{
    public static PlayerShopManager instance;

    public int TempGold { get; private set; }
    public int RerollCount { get; private set; }
    public int RerollCost => BaseRerollCost + RerollCount;

    public List<DiceData> TempDices = new();
    public List<BattleItemSo> TempItems = new();

    [Header("Settings")]
    [SerializeField] private int baseRerollCost = 1;
    public int BaseRerollCost => baseRerollCost;
    [SerializeField] private string ShopBGMKey;

    [Header("UI References")]
    [SerializeField] private GameObject shopCanvas;
    [SerializeField] private RectTransform shopPanel;
    [SerializeField] private ShopPanelAnimator shopAnimator;

    public event System.Action<int> OnGoldChanged;

    [SerializeField] private DiceData defaultDice;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        OpenWithAnimation();
        AudioManager.instance.PlayBgm(ShopBGMKey);
    }

    public void Open()
    {        
        
        var Resource = ResourceManager.instance;
        var Deck = PlayerDeck.instance;
        var Item = ItemManager.instance;

        TempGold = Resource.gold;
        RerollCount = 0;

        TempDices = new List<DiceData>(Deck.inventory);
        TempItems = new List<BattleItemSo>(Item.items);

        IsOpen = true;
        OnGoldChanged?.Invoke(TempGold);

    }

    public async void OpenWithAnimation()
    {       
        Open();
        shopCanvas.SetActive(true);
        if (shopAnimator != null && shopAnimator.gameObject != null)
        {
            await shopAnimator.Show();
        }
        else
        {
            Debug.Log("shopAnimator 또는 gameObject가 null입니다!");
        }
    }

    public void Commit()
    {
        if (!IsOpen)
        {
            Debug.Log("Commit 호출됐지만 상점이 열려있지 않습니다.");
            return;
        }

        var Resource = ResourceManager.instance;
        var Deck = PlayerDeck.instance;
        var Item = ItemManager.instance;

        Resource.gold = TempGold;
        Deck.inventory = new List<DiceData>(TempDices);
        Item.items = new List<BattleItemSo>(TempItems);

        Resource.Save();
        Deck.Save();
        Item.Save();
        IsOpen = false;
    }

    public async UniTask CommitWithAnimation()
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

    public bool TryPurchaseDice(DiceData dice)
    {
        int cost = dice.gold;
        if (!HasEnoughGold(cost)) return false;

        SpendGold(cost);
        TempDices.Add(dice);
        return true;
    }

    public bool TryPurchaseItem(BattleItemSo item)
    {
        int cost = item.gold;
        if (!HasEnoughGold(cost)) return false;

        SpendGold(cost);
        TempItems.Add(item);
        return true;
    }

    public void SellDice(DiceData dice, int slotIndex, int sellPrice)
    {
        TempDices[slotIndex] = defaultDice;
        GainGold(sellPrice);
    }

    public void SellItem(BattleItemSo item, int slotIndex, int sellPrice)
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
