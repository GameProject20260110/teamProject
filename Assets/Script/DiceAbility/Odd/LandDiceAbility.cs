using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/land")]
public class LandDiceAbility : DiceData
{
    public int bonusScore = 2;
    

    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;

        foreach (var dice in allDice)
        {
            if(dice != null && !dice.IsCurrentEven)
            {
                int add = dice.scoreValue *= currentBonusScore;
                totalScore += add;
                events.Add(new ScoreEventData(ScoreEventData.Type.Multiplier, dice.diceIndex, totalScore, $"Land! x{bonusScore}"));
            }
        }
    }
}
