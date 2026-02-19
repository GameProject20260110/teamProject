using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Ax")]
public class Ax : ItemSo
{
    public int bonusRoundScore = 2;
    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        totalScore += bonusRoundScore;

        foreach(var dice in allDice)
        {
            dice.isForceOdd = true;
        }
    }
}
