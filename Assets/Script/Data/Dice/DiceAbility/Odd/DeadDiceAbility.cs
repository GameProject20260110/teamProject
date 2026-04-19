using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/dead")]
public class DeadDiceAbility : DiceData
{
    public int bonusScore = 0;

    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        List<int> oddInDice = new List<int>();
        foreach (var dice in allDice)
        {
            if (dice != null && !dice.IsCurrentEven)
            {
                oddInDice.Add(dice.diceIndex);
            }
        }
        if (oddInDice.Count == 0) return;

        int multiplier = oddInDice.Count * myState.multiBonusScore + myState.plusBonusScore;
        totalScore *= multiplier;

        events.Add(new ScoreEventData(ScoreEventData.Type.TriggerDice, myState.diceIndex)
        {
            effectName = abilityName,
            effectDesc = this.effectDesc
        });
        events.Add(new ScoreEventData(ScoreEventData.Type.TargetBuff, oddInDice.ToArray(), totalScore, "Dead"));
        Bow.TryTrigger(ref totalScore, events);
    }
}
