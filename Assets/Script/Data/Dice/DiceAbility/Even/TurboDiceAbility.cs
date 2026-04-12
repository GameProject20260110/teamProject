using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/turbo")]
public class TurboDiceAbility : DiceData
{
    public int bonusScore = 3;

    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        if (myState.diceData != this) return;

        if(myState.IsCurrentEven)
        {
            int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;
            foreach(var dice in allDice)
            {
                if(dice != null && dice.IsCurrentEven)
                {
                    int score = dice.scoreValue * currentBonusScore;
                    int diff = dice.ApplyDiceScoreChange(score);
                    if(diff != 0)
                    {
                        totalScore += diff;
                        events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"x{currentBonusScore}", dice.scoreValue)
                        {
                            effectName = abilityName,
                            effectDesc = this.effectDesc
                        });
                    }
                }
            }
        }
        Bow.TryTrigger(ref totalScore, events);
    }
}
