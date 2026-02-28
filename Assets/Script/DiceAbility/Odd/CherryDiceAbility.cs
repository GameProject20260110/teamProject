using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/cherry")]
public class CherryDiceAbility : DiceData
{
    public int bonusScore = 5;

    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;

        if (!myState.IsCurrentEven)
        {
            foreach(var dice in allDice)
            {
                if (dice != null && !dice.IsCurrentEven)
                {
                    dice.scoreValue += currentBonusScore;
                    totalScore += currentBonusScore;
                    events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"Cherry! +{currentBonusScore}"));
                }
            }
        }
    }
}
