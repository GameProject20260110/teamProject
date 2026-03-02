using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuySpecialSlot : MonoBehaviour
{
    [SerializeField] private int gold = 8;
    [SerializeField] private int level = 1;

    [Header("≈ÿΩ∫∆Æ")]
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
        foreach(var slot in DiceSlot)
        {
            if (!slot.hasSpecialSlot && PlayerManager.instance.gold >= gold && level < 6)
            {
                slot.SetSpecialSlot(true); 
                PopupManager.instance.BuyItems(gold);
                PlayerManager.instance.SpecialSlots[level] = true;
                StateUpdate();
                TextUpdate();               
                return;
            }
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
