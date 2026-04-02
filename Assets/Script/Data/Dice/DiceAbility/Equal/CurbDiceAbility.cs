using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/curb")]
public class CurbDiceAbility : DiceData
{
    
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int[] localBonus = new int[7];
        
        foreach (var dice in allDice)
        {
            localBonus[dice.modifiedValue]++;            
        }

        foreach(var dice in allDice)
        {
            int score = localBonus[dice.modifiedValue] * dice.modifiedValue;
            int diff = dice.ApplyDiceScoreChange(score);

            if(diff != 0)
            {
                totalScore += diff;
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, totalScore, diff > 0 ? $"+{diff}" : "", dice.scoreValue)
                {
                    effectName = abilityName,
                    effectDesc = "눈금이 같은 주사위 수 x 동일 눈금 점수"
                });
                Bow.TryTrigger(ref totalScore, events);
            }
        }
    }

}
