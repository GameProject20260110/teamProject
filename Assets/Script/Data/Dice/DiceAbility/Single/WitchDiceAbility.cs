using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/witch")]
public class WitchDiceAbility : DiceData
{
    public int bonusScore = 3;
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        if (myState == null) return;
        totalScore *= bonusScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.Multiplier, myState.diceIndex, totalScore, $"Witch! Round x {bonusScore}"));

        int startIndex = myState.diceIndex + 1;

        if(startIndex < allDice.Count)
        {
            int randomTargetIndex = Random.Range(startIndex, allDice.Count);
            allDice[randomTargetIndex].isIgnored = true;
            events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, randomTargetIndex, totalScore, "Slience"));
        }
    }

}
