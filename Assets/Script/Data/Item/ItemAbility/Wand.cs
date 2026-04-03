using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Wand")]
public class Wand : ItemSo
{
    public int bonusScore = 2;
    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events, int itemIndex = - 1)
    {
        List<int> targetInDice = new List<int>();
        foreach (var dice in allDice)
        {
            if(dice.diceData.diceNum == 0 && dice.diceData.type == ScoreManager.DiceType.None)
            {
                int score = dice.scoreValue + bonusScore;
                int diff = dice.ApplyDiceScoreChange(score);
                if (diff != 0)
                {
                    totalScore += diff;
                    targetInDice.Add(dice.diceIndex);
                }
            }
        }
        if(targetInDice.Count > 0)
        {
            events.Add(new ScoreEventData(ScoreEventData.Type.ItemEffect, itemIndex, totalScore) 
            {
                effectName = itemName,
                effectDesc = $"효과가 없는 주사위 점수 +{bonusScore}"
            });
        }
    }
}
