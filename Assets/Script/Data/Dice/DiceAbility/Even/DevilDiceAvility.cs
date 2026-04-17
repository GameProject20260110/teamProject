using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/Devil")]
public class DevilDiceAbility : DiceData
{
    public int bonusScore = 5;
    public int panelty = 3;

    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentPlus = bonusScore * myState.multiBonusScore + myState.plusBonusScore;

        events.Add(new ScoreEventData(ScoreEventData.Type.TriggerDice, myState.diceIndex)
        {
            effectName = abilityName,
            effectDesc = this.effectDesc
        });

        foreach(var dice in allDice)
        {
            int score = dice.IsCurrentEven ? dice.scoreValue + currentPlus : dice.scoreValue - panelty;
            int diff = dice.ApplyDiceScoreChange(score);
            if(diff != 0)
            {
                totalScore += diff;
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, dice.IsCurrentEven ? $"+{currentPlus}" : $"-{panelty}", dice.scoreValue));
            }
        }
        Bow.TryTrigger(ref totalScore, events);
    }
}