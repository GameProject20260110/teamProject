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

        foreach(var dice in allDice)
        {
            dice.modifiedValue = localMaxValue;
            dice.change = true;
            events.Add(new ScoreEventData(ScoreEventData.Type.ChangeFace, dice.diceIndex, localMaxValue, $"Change {localMaxValue}") { effectName = abilityName, effectDesc = "모든 주사위를 현재 가장 높은 눈금으로 변경" });

            int diff = dice.ApplyDiceScoreChange(localMaxValue);
            if(diff != 0)
            {
                totalScore += diff;
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, diff > 0 ? $"+{diff}" : "", dice.scoreValue));
                Bow.TryTrigger(ref totalScore, events);
            }
        }
        ChangeModi(myState, allDice, ref totalScore, events);
    }
}
