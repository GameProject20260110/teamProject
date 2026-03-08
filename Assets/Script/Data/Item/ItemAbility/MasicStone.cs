using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/MasicStone")]
public class MasicStone : ItemSo
{
    public int bonusScore = 2;
    public override void RoundStart(List<DiceState> allDice, ref int totalSoce, List<ScoreEventData> events)
    {
        totalSoce *= bonusScore;
        allDice[0].isIgnored = true;
        events.Add(new ScoreEventData(ScoreEventData.Type.Multiplier, -1, 0, $"MasicStone x{bonusScore}"));
    }
}
