using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/bam")]
public class BamDiceAbility : DiceData
{
    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, List<ScoreEventData> events)
    {
        int localMaxValue = 0;       
        foreach (var dice in allDice)
        {
            if(dice.modifiedValue > localMaxValue) localMaxValue = dice.modifiedValue;
        }

        foreach(var dice in allDice)
        {
            dice.modifiedValue = localMaxValue;
            dice.scoreValue = localMaxValue;
            dice.change = true;
            events.Add(new ScoreEventData(ScoreEventData.Type.ChangeFace, dice.diceIndex, localMaxValue, "Change Bam!"));
        }

        ChangeModi(myState, allDice, events);
    }

}
