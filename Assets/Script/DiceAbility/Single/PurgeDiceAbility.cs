using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/purge")]
public class PurgeDiceAbility : DiceData
{
    public int bonusScore = 3;

    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;

        foreach (var dice in allDice)
        {
            if (dice == null) return;
            dice.scoreValue += currentBonusScore;
            totalScore += currentBonusScore;
            events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"Purge +{bonusScore}"));
        }       
    }
}
