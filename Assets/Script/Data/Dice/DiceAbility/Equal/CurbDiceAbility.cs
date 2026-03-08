using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/curb")]
public class CurbDiceAbility : DiceData
{
    
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int[] localBonus = new int[7];
        
        foreach (var dice in allDice)
        {
            localBonus[dice.modifiedValue]++;            
        }

        foreach(var dice in allDice)
        {
            int score = localBonus[dice.modifiedValue] * dice.modifiedValue;
            totalScore += score;
            events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"curb +{score}"));
        }
    }

}
