using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/dead")]
public class DeadDiceAbility : DiceData
{
    public int bonusScore = 0;

    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int deadCount = 0;

        foreach (var dice in allDice)
        {
            if (dice != null && !dice.IsCurrentEven)
            {
                deadCount++;

                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, "Dead bonus"));
            }
        }
        int finalMultiplier = Mathf.Min(1, deadCount * myState.multiBonusScore + myState.plusBonusScore);
        totalScore *= finalMultiplier;
        events.Add(new ScoreEventData(ScoreEventData.Type.GlobalBuffs, -1, totalScore, $"Dead x {finalMultiplier}"));
    }
}
