using UnityEngine;

public class DiceSetup : MonoBehaviour
{
    private bool UseTestMode => TestModeManager.instance != null && TestModeManager.instance.isTestModeActive;

    public void Setup(Dice[] diceScript, DiceData defaultDice) 
    {
        if (diceScript == null) return;

        DiceData fallbackDice = defaultDice;
        if(PlayerManager.instance != null && PlayerManager.instance.defaultDice != null)
        {
            fallbackDice = PlayerManager.instance.defaultDice;
        }
        for(int i = 0; i < diceScript.Length; i++)
        {
            if (diceScript[i] == null) continue;
            DiceData dataToUse = GetDiceData(i, fallbackDice);
            diceScript[i].Initialize(i, dataToUse);
        }
    }

    private DiceData GetDiceData(int index, DiceData fallbackDice)
    {
        if(UseTestMode)
        {
            var tm = TestModeManager.instance;
            if(index < tm.testDiceSlot.Length && tm.testAbilities[index] != null)
            {
                return tm.testAbilities[index];
            }
        }
        else
        {
            var pm = PlayerManager.instance;
            if(pm != null && index < pm.SpecialSlots.Length && pm.SpecialSlots[index] && pm.dices != null && index < pm.dices.Count && pm.dices[index] != null)
            {
                return pm.dices[index];
            }
        }
        return fallbackDice;
    }
}
