using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Mace")]
public class Mace : ItemSo
{
    public int bonusRoundScore = 1;

    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        totalScore += bonusRoundScore;

        foreach(var dice in allDice)
        {
            if(dice != null)
            {
                dice.isForceEven = true;
            }
        }

    }
}
