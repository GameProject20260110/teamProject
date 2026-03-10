using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/sky")]
public class SkyDiceAbility : DiceData
{
    public int bonusScore = 2;

    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;
        foreach (var dice in allDice)
        {
            if (dice != null && dice.IsCurrentEven)
            {
                int add = dice.scoreValue * (currentBonusScore - 1);
                dice.scoreValue *= currentBonusScore;
                totalScore += add;
                events.Add(new ScoreEventData(ScoreEventData.Type.Multiplier, dice.diceIndex, totalScore, $"Sky x{currentBonusScore}", dice.scoreValue));
            }
        }
    }
}