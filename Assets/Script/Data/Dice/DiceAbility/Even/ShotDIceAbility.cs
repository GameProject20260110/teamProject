using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/Shot")]
public class ShotDiceAbility : DiceData
{
    public int bonusScore = 3;

    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;

        foreach(var dice in allDice)
        {
            if(dice != null && dice.IsCurrentEven)
            {
                int score = dice.scoreValue + currentBonusScore;
                int diff = dice.ApplyDiceScoreChange(score);
                if(diff != 0)
                {
                    totalScore += diff;
                    events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"+{currentBonusScore}", dice.scoreValue)
                    {
                        effectName = abilityName,
                        effectDesc = "¸ðµç Â¦¼ö ´«±ÝÀÇ Á¡¼ö +3"
                    });
                }
            }
        }
    }
}
