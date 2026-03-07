using TMPro;
using UnityEngine;

public class BuySpecialSlot : MonoBehaviour
{
    [SerializeField] private int gold = 8;
    [SerializeField] private int level = 1;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private ItemSlot[] DiceSlot;

    private void Start()
    {
        for (int i = 1; i < PlayerManager.instance.SpecialSlots.Length; i++)
        {
            if (PlayerManager.instance.SpecialSlots[i]) StateUpdate();
        }
        TextUpdate();
    }

    public void OnClickBuy()
    {
        if (level >= 6) return;

        int finalPrice = LuckyStone.CalcDiscount(gold);

        foreach(var slot in DiceSlot)
        {
            if (slot.hasSpecialSlot) continue;

            bool success = PlayerShopManager.instance.TryPurchaseSpecialSlot(finalPrice, level);
            if (!success)
            {
                Debug.Log("골드 부족 — 슬롯 구매 불가");
                return;
            }

            slot.SetSpecialSlot(true);
            StateUpdate();
            TextUpdate();
            return;
        }
    }

    private void StateUpdate()
    {        
        level++;
        gold += 2;       
    }

    private void TextUpdate()
    {
        levelText.text = $"Level: {level}";
        goldText.text = $"Gold: {gold}";
    }
}
