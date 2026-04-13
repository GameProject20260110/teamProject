using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/bubble")]
public class BubbleDiceAbility : DiceData
{
    public int bonusScore = 3;

    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;

        foreach(var dice in allDice)
        {
            if(dice.IsCurrentEven)
            {
                dice.isForceOdd = true;
                int score = dice.scoreValue + currentBonusScore;
                int diff = dice.ApplyDiceScoreChange(score);
                if(diff != 0)
                {
                    totalScore += diff;
                    events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"+{currentBonusScore}", dice.scoreValue)
                    {
                        effectName = abilityName,
                        effectDesc = this.effectDesc
                    });
                }
            }
        }
        Bow.TryTrigger(ref totalScore, events);
    }
}
