using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Clock")]
public class Clock : ItemSo
{
    public int bonusRerollCount = 1;
    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events, int itemIndex = -1)
    {
        events.Add(new ScoreEventData(ScoreEventData.Type.GainReroll, -1, totalScore)
        {
            effectName = itemName,
            effectDesc = $"Àç±¼¸² +{bonusRerollCount}"
        });
    }
}
