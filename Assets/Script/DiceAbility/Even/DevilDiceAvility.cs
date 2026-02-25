using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/Devil")]
public class DevilDiceAbility : DiceData
{
    public int bonusScore = 5;
    public int panelty = 3;

    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentPlus = bonusScore * multiBonusScore + plusBonusScore;

        foreach(var dice in allDice)
        {
            if(dice.IsCurrentEven)
            {
                totalScore += currentPlus;
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"Devil +{currentPlus}"));
            }
            else
            {
                totalScore -= panelty;
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"Devil -{panelty}"));
            }
        }
    }
}