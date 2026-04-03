using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Ax")]
public class Ax : ItemSo
{
    public int bonusRoundScore = 2;
    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events, int itemIndex = -1)
    {
        foreach(var dice in allDice)
        {
            if(dice != null)
            {
                dice.isForceOdd = true;
            }
        }
        totalScore += bonusRoundScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.ItemEffect, itemIndex, totalScore)
        {
            effectName = itemName,
            effectDesc = $"모든 주사위 홀수로 변경, 점수 +{bonusRoundScore}점"
        });
    }
}
