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
                    int score = dice.scoreValue + currentBonusScore;
                    int diff = dice.ApplyDiceScoreChange(score);
                    if(diff != 0)
                    {
                        totalScore += diff;
                        events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"+{currentBonusScore}", dice.scoreValue)
                        {
                            effectName = abilityName,
                            effectDesc = $"¸ðµç È¦¼ö ´«±Ý Á¡¼ö +{currentBonusScore}"
                        });
                    }
                    
                }
            }
        }
    }
}
