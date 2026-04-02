using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Rapier")]
public class Rapier : ItemSo
{

    public int bonusScore = 5;
    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events, int itemIndex = -1)
    {
        totalScore += bonusScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.ItemEffect, itemIndex, totalScore) { effectName = "레이피어", effectDesc = "라운드 점수 + 5" });
    }
}
