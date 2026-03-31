using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/bubble")]
public class BubbleDiceAbility : DiceData
{
    public int bonusScore = 3;

    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;

        if(myState.IsCurrentEven)
        {
            myState.isForceOdd = true;
            int score = myState.scoreValue + currentBonusScore;
            int diff = myState.ApplyDiceScoreChange(score);

            if(diff != 0)
            {
                totalScore += diff;
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, myState.diceIndex, totalScore, "Bubble", myState.scoreValue)
                {
                    effectName = abilityName,
                    effectDesc = "모두 홀수 취급하고 이 주사위 눈금 +3"
                });
            }
        }
    }
}
