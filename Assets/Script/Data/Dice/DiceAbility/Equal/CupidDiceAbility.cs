using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/cupid")]
public class CupidDiceAbility : DiceData
{
    public int bonusScore = 2;
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int currentBonusScore = bonusScore * myState.multiBonusScore + myState.plusBonusScore;

        int[] localBonus = new int[7];

        foreach (var dice in allDice)
        {
            localBonus[dice.modifiedValue]++;
        }

        List<int> targetInDices = new List<int>();
        foreach(var dice in allDice)
        {
            if (localBonus[dice.modifiedValue] >= 2)
            {
                int add = dice.scoreValue * (currentBonusScore - 1);
                dice.scoreValue *= currentBonusScore;
                totalScore += add;
                targetInDices.Add(dice.diceIndex);
            }
        }
        if(targetInDices.Count > 0)
        {
            events.Add(new ScoreEventData(ScoreEventData.Type.TargetBuff, targetInDices.ToArray(), totalScore, $"x{currentBonusScore}") { effectName = abilityName, effectDesc = "동일 눈금 주사위 점수 *2", targetIndex = myState.diceIndex});
        }
    }
}
