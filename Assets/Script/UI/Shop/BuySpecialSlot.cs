using TMPro;
using UnityEngine;

public class BuySpecialSlot : MonoBehaviour
{
    [Header("Upgrade Settings")]
    [SerializeField] private int baseGold = 8;
    [SerializeField] private int goldIncrement = 2;
    [SerializeField] private int maxLevel = 1;

    [Header("Current State")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentGold = 8;
    private int nextSlotIndex = 1;

    [Header("UI Referencecs")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private ItemSlot[] diceSlots;

    private void Start()
    {
        InitializeFromPlayerData();
        UpdateUI();
    }

    private void InitializeFromPlayerData()
    {
        // PlayerManager의 SpecialSlots 상태에 따라 레벨 동기화
        for (int i = 1; i < PlayerManager.instance.SpecialSlots.Length; i++)
        {
            if (PlayerManager.instance.SpecialSlots[i])
            {
                LevelUp();
                nextSlotIndex = i;
            }
                
        }
    }

    public void OnClickBuy()
    {
        if (currentLevel >= maxLevel)
        {
            Debug.Log(currentLevel);
            Debug.Log("최대 레벨 도달 — 더 이상 구매 불가");
            return;
        }

        int finalPrice = LuckyStone.CalcDiscount(currentGold);

        // 골드 검증 및 구매
        bool success = PlayerShopManager.instance.TryPurchaseSpecialSlot(finalPrice, currentLevel);
        if (!success)
        {
            Debug.Log("골드 부족 — 슬롯 구매 불가");
            return;
        }

        // 슬롯 활성화 및 레벨업
        diceSlots[nextSlotIndex].SetSpecialSlot(true);
        nextSlotIndex++;
        LevelUp();
        UpdateUI();
    }

    private void LevelUp()
    {
        currentLevel++;
        currentGold += goldIncrement;
    }

    private void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"Level: {currentLevel}";

        if (goldText != null)
        {
            int displayGold = LuckyStone.CalcDiscount(currentGold);
            goldText.text = $"Gold: {displayGold}";
        }
    }
}
