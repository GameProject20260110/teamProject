using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/sun")]
public class SunDiceAbility : DiceData
{
    public int bonusScore = 2;
    
    public override void OnRuleEffect(DiceState myState, List<DiceState> allDice, List<ScoreEventData> events)
    {
        foreach (var dice in allDice)
        {
            if (dice == myState) continue;
            if(dice != null && dice.currentType == ScoreManager.DiceType.Odd)
            {
                dice.multiBonusScore *= bonusScore;
            }   
        }
    }
}
