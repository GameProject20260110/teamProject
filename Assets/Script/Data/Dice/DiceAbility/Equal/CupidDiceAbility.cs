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
        List<(int diceIndex, int scoreValue)> addScoreList = new List<(int diceIndex, int scoreValue)>();
        foreach(var dice in allDice)
        {
            if (localBonus[dice.modifiedValue] >= 2)
            {
                int score = dice.scoreValue * currentBonusScore;
                int diff = dice.ApplyDiceScoreChange(score);
                if(diff != 0)
                {
                    totalScore += diff;
                    targetInDices.Add(dice.diceIndex);
                    addScoreList.Add((dice.diceIndex, dice.scoreValue));
                }
                
            }
        }
        if(targetInDices.Count > 0)
        {
            events.Add(new ScoreEventData(ScoreEventData.Type.TargetBuff, targetInDices.ToArray(), totalScore, $"x{currentBonusScore}") { effectName = abilityName, effectDesc = "���� ���� �ֻ��� ���� *2", targetIndex = myState.diceIndex});

            foreach(var (diceIndex, scoreValue) in addScoreList)
            {
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, diceIndex, totalScore, $"x{currentBonusScore}", scoreValue));
            }   
        }
    }
}
