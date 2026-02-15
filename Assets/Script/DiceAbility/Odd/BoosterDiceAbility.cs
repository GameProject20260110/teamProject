using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/booster")]
public class BoosterDiceAbility : DiceData
{
    public int bonusScore = 3;
    
    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;

        int count = 0;
        foreach (var dice in allDice)
        {
            if (dice != null && !dice.IsCurrentEven)
            {
                count++;
            }
        }

        if (count >= 3)
        {
            totalScore += (currentBonusScore * allDice.Count);
            events.Add(new ScoreEventData(ScoreEventData.Type.GlobalBuffs, -1, 0, $"Booster All +{currentBonusScore}"));

            if (!GameManager.instance.hasUsedPlusReroll) 
            {
                GameManager.instance.hasUsedPlusReroll = true;
                GameManager.instance.CurrentRerollCount++;
                events.Add(new ScoreEventData(ScoreEventData.Type.GlobalBuffs, -1, 0, "Booster +1 Reroll"));
            }
        }
    }
}
