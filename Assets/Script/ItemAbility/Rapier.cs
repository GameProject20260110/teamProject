using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Rapier")]
public class Rapier : ItemSo
{

    public int bonusScore = 5;
    public override void RoundStart(List<DiceState> allDice, ref int totalSocre, List<ScoreEventData> events)
    {
        totalSocre += bonusScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.GlobalBuffs, -1, 0, $"Rapier +{bonusScore}"));
    }
}
