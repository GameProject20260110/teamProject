using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/fire")]
public class FireDiceAbility : DiceData
{
    public int bonusScore = 3;

    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> scoreEvent)
    {
        totalScore *= bonusScore;
    }
}
