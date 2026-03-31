using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/bubble")]
public class BubbleDiceAbility : DiceData
{
    public int bonusScore = 3;

    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int finalScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;

        if(myState.IsCurrentEven)
        {
            myState.isForceOdd = true;
            myState.scoreValue += finalScore;

            events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, myState.diceIndex, 0, "Bubble", myState.scoreValue));
        }

        
    }

}
