using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Sword")]
public class Sword : ItemSo
{
    public int bonusScore = 15;

    private void OnEnable()
    {
        isConsumable = true;
    }
    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events, int itemIndex = - 1)
    {
        totalScore += bonusScore;

        events.Add(new ScoreEventData(ScoreEventData.Type.ItemEffect, itemIndex, totalScore)
        {
            effectName = itemName,
            effectDesc = $"라운드 점수 + {bonusScore}"
        });
    }
}
