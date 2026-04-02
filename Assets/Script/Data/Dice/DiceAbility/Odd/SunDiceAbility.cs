using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/sun")]
public class SunDiceAbility : DiceData
{
    public int bonusScore = 2;
    
    public override void OnRuleEffect(DiceState myState, List<DiceState> allDice, List<ScoreEventData> events)
    {
        List<DiceData> localUseData = new List<DiceData>();

        foreach (var dice in allDice)
        {
            if (dice == myState) continue;
            if(dice != null && dice.currentType == ScoreManager.DiceType.Odd && !localUseData.Contains(dice.diceData))
            {
                dice.multiBonusScore *= bonusScore;
                localUseData.Add(dice.diceData);
            }   
        }
    }
}
