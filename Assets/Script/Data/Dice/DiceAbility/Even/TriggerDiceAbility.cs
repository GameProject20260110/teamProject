using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/trigger")]
public class TriggerDiceAbility : DiceData
{
    public int bonusScore = 2;

    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        bool isConditionMet = true;

        foreach(var dice in allDice)
        {
            if(dice != null && !dice.IsCurrentEven)
            {
                isConditionMet = false;
                break;
            }
        }

        if (isConditionMet)
        {
            totalScore *= bonusScore;
            events.Add(new ScoreEventData(ScoreEventData.Type.GlobalBuffs, -1, totalScore, $"Trigger! x{bonusScore}"));
        }
    }
}
