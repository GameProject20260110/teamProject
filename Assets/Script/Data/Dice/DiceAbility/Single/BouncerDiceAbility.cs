using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/bouncer")]
public class BouncerDiceAbility : DiceData
{
    public int plusScore = 10;
    public int bonusScore = 2;
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {

        totalScore += plusScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, myState.diceIndex, totalScore, $"+{plusScore}"));
        totalScore *= bonusScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.Multiplier, myState.diceIndex, totalScore, $"x{bonusScore}"));
    }
}
