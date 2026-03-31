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
                int score = dice.scoreValue * currentBonusScore;
                int diff = dice.ApplyDiceScoreChange(score);
                if(diff != 0)
                {
                    totalScore += diff;
                    events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"x{currentBonusScore}", dice.scoreValue)
                    {
                        effectName = abilityName,
                        effectDesc = $"¸ðµç È¦¼ö ´«±ÝÀÇ Á¡¼ö x{currentBonusScore}"
                    });
                }
            }
        }
    }
}
