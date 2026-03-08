using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/captain")]
public class CaptainDiceAbility : DiceData
{

    
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int count = 0;

        foreach (var dice in allDice)
        {
            if(dice.modifiedValue == myState.modifiedValue)
            {
                count++;
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"Captain!"));
            }
        }

        totalScore *= count;
        events.Add(new ScoreEventData(ScoreEventData.Type.GlobalBuffs, -1, totalScore, $"Captain! x{count}"));
    }
}
