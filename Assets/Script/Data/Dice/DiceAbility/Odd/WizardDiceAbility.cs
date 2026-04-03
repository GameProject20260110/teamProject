using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/wizard")]
public class WizardDiceAbility : DiceData
{
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int oddCount = 0;
        foreach (var dice in allDice)
        {
            if (!dice.IsCurrentEven) oddCount++;
        }

        if (oddCount == 0) return;

        foreach(var dice in allDice)
        {
            int score = dice.scoreValue + oddCount;
            int diff = dice.ApplyDiceScoreChange(score);

            if(diff != 0)
            {
                totalScore += diff;
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, $"+{oddCount}", dice.scoreValue)
                {
                    effectName = abilityName,
                    effectDesc = "모든 주사위 점수 + 홀수 주사위 수"
                });
                Bow.TryTrigger(ref totalScore, events);
            }
        }
    }
}
