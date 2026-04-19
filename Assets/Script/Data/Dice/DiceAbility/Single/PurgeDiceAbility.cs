using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/purge")]
public class PurgeDiceAbility : DiceData
{
    public int bonusScore = 3;

    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.TriggerDice, myState.diceIndex)
        {
            effectName = abilityName,
            effectDesc = this.effectDesc
        });
        foreach (var dice in allDice)
        {
            if (dice == null) continue;
            int score = dice.scoreValue + currentBonusScore;
            int diff = dice.ApplyDiceScoreChange(score);
            if (diff != 0)
            {
                totalScore += diff;
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"+{bonusScore}", dice.scoreValue));
            }
        }
        Bow.TryTrigger(ref totalScore, events);
    }
}
