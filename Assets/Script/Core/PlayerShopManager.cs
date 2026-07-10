using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class PlayerShopManager : MonoBehaviour
{
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

    private AudioManager _audioManager;
    private ResourceManager _resourceManager;
    private PlayerDeck _playerDeck;
    private ItemManager _itemManager;

    [Inject]
    public void Construct(AudioManager audioManager, ResourceManager resourceManager, PlayerDeck playerDeck, ItemManager itemManager)
    {
        _audioManager = audioManager;
        _resourceManager = resourceManager;
        _playerDeck = playerDeck;
        _itemManager = itemManager;
    }

    private void Start()
    {
        OpenWithAnimation();
        _audioManager.PlayBgm(ShopBGMKey);
    }

    public void Open()
    {        
        TempGold = _resourceManager.gold;
        RerollCount = 0;

        TempDices = new List<DiceData>(_playerDeck.inventory);
        TempItems = new List<BattleItemSo>(_itemManager.items);

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
        _resourceManager.gold = TempGold;
        _playerDeck.inventory = new List<DiceData>(TempDices);
        _itemManager.items = new List<BattleItemSo>(TempItems);
        _resourceManager.Save();
        _playerDeck.Save();
        _itemManager.Save();
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

    public void SetDiceAtSlot(int slotIndex, DiceData data) => TempDices[slotIndex] = data;

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
