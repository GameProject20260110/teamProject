using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/cupid")]
public class CupidDiceAbility : DiceData
{
    public int bonusScore = 2;
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;

        int[] localBonus = new int[7];

        foreach (var dice in allDice)
        {
            localBonus[dice.modifiedValue]++;
        }

        foreach (var dice in allDice)
        {
            if(localBonus[dice.modifiedValue] >= 2)
            {
                int add = dice.scoreValue * (currentBonusScore - 1);
                dice.scoreValue *= currentBonusScore;
                totalScore += add;
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"Cupid +{add}"));
            }
        }
    }

}
