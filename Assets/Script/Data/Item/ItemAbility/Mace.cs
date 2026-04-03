using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Mace")]
public class Mace : ItemSo
{
    public int bonusRoundScore = 1;

    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events, int itemIndex = -1)
    {
        foreach(var dice in allDice)
        {
            if(dice != null)
            {
                dice.isForceEven = true;
            }
        }
        totalScore += bonusRoundScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.ItemEffect, itemIndex, totalScore)
        {
            effectName = itemName,
            effectDesc = $"모든 주사위 짝수로 변경, 점수 +{bonusRoundScore}점"
        });
    }
}
