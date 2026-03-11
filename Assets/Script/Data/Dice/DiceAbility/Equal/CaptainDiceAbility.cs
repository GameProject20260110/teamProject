using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/captain")]
public class CaptainDiceAbility : DiceData
{
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        if (myState.diceData != this) return;
        int count = 0;

        foreach (var dice in allDice)
        {
            if(dice.modifiedValue == myState.modifiedValue)
            {
                count++;
            }
        }
        if (count <= 1) return;

        totalScore *= count;
        foreach(var dice in allDice)
        {
            if(dice.modifiedValue == myState.modifiedValue)
            {
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"Captain!"));  
            }
        }
        events.Add(new ScoreEventData(ScoreEventData.Type.GlobalBuffs, -1, totalScore, $"Captain! x{count}"));
    }
}
