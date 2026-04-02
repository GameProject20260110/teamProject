using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/booster")]
public class BoosterDiceAbility : DiceData
{
    public int bonusScore = 3;
    
    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;

        int count = 0;
        foreach (var dice in allDice)
        {
            if (dice != null && !dice.IsCurrentEven)
            {
                count++;
            }
        }

        if (count >= 3)
        {
            if (!GameManager.instance.hasUsedPlusReroll)
            {
                GameManager.instance.hasUsedPlusReroll = true;
                events.Add(new ScoreEventData(ScoreEventData.Type.GainReroll, -1, totalScore, "Reroll +1"));
            }

            foreach (var dice in allDice)
            {
                int score = dice.scoreValue + currentBonusScore;
                int diff = dice.ApplyDiceScoreChange(score);
                if(diff != 0)
                {
                    totalScore += diff;
                    events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"+{currentBonusScore}", dice.scoreValue)
                    {
                        effectName = abilityName,
                        effectDesc = $"모든 주사위 점수 +{currentBonusScore} 재굴림 +1"
                    });
                    Bow.TryTrigger(ref totalScore, events);
                }
            }

            
        }
    }
}
