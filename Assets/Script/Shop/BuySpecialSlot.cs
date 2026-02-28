using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuySpecialSlot : MonoBehaviour
{
    [SerializeField] private int gold = 8;
    [SerializeField] private int level = 1;

    [Header("ÅØ½ºÆ®")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private ItemSlot[] DiceSlot;

    public void OnClickBuy()
    {
        foreach(var slot in DiceSlot)
        {
            if (!slot.hasSpecialSlot && PlayerManager.instance.gold >= gold)
            {
                slot.SetSpecialSlot(true);
                PlayerManager.instance.SpecialSlots[level - 1] = true;
                PopupManager.instance.BuyItems(gold);
                StateUpdate();
                return;
            }
        }
    }

    private void StateUpdate()
    {
        level++;
        gold += 2;
        levelText.text = $"Level: {level}";
        goldText.text = $"Gold: {gold}";
    }
}
