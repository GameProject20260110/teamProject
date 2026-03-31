using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/trigger")]
public class TriggerDiceAbility : DiceData
{
    public int bonusScore = 2;

    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        foreach(var dice in allDice)
        {
            if(dice != null && !dice.IsCurrentEven) return;
        }

        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;
        totalScore *= currentBonusScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.GlobalBuff, -1, totalScore, $"x{currentBonusScore}" )
        {
            effectName = abilityName,
            effectDesc = "" 
        });
    }
}
