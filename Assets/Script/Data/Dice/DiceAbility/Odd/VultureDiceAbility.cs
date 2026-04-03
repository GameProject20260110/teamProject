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
            events.Add(new ScoreEventData(ScoreEventData.Type.GlobalBuff, -1, totalScore, $"x{currentBonusScore}")
            {
                effectName = abilityName,
                effectDesc = "라운드 점수 x2"
            });
            Bow.TryTrigger(ref totalScore, events);
        }
    }
}
