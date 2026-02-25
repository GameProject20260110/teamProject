using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/turbo")]
public class TurboDiceAbility : DiceData
{
    public int bonusScore = 3;

    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        if(myState.IsCurrentEven)
        {
            int currentBonusScore = bonusScore * myState.multiBonusScore + plusBonusScore;
            foreach(var dice in allDice)
            {
                if(dice != null && dice.IsCurrentEven)
                {
                    int add = dice.scoreValue * currentBonusScore;
                    totalScore += add;
                    events.Add(new ScoreEventData(ScoreEventData.Type.Multiplier, dice.diceIndex, totalScore, $"Turbo x{currentBonusScore}"));
                }
            }
        }
    }

}
