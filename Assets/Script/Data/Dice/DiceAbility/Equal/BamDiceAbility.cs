using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/bam")]
public class BamDiceAbility : DiceData
{
    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int localMaxValue = 0;       
        foreach (var dice in allDice)
        {
            if(dice.modifiedValue > localMaxValue) localMaxValue = dice.modifiedValue;
        }
        events.Add(new ScoreEventData(ScoreEventData.Type.TriggerDice, myState.diceIndex)
        {
            effectName = abilityName,
            effectDesc = this.effectDesc
        });

        bool isFirst = true;
        foreach(var dice in allDice)
        {
            dice.modifiedValue = localMaxValue;
            dice.change = true;
            int diff = dice.ApplyDiceScoreChange(localMaxValue);
            if (diff != 0) totalScore += diff;
            
            events.Add(new ScoreEventData(ScoreEventData.Type.ChangeFace, dice.diceIndex, totalScore, $"Change {localMaxValue}", localMaxValue, triggerIndex : isFirst ?  myState.diceIndex : -1));
            isFirst = false;
        }
        Bow.TryTrigger(ref totalScore, events);
        ChangeModi(myState, allDice, ref totalScore, events);
    }
}
