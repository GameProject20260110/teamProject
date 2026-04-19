using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/vulture")]
public class VultureDiceAbility : DiceData
{
    public int bonusScore = 2;

    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;
        if (!myState.IsCurrentEven)
        {
            totalScore *= currentBonusScore;
            events.Add(new ScoreEventData(ScoreEventData.Type.TriggerDice, myState.diceIndex)
            {
                effectName = abilityName,
                effectDesc = this.effectDesc
            });
            events.Add(new ScoreEventData(ScoreEventData.Type.GlobalBuff, -1, totalScore, $"x{currentBonusScore}"));
            Bow.TryTrigger(ref totalScore, events);
        }
    }
}
