using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Clock")]
public class Clock : ItemSo
{
    public int bonusRerollCount = 1;

    private void OnEnable()
    {
        isConsumable = true;
    }

    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events, int itemIndex = -1)
    {
        if(GameManager.instance != null)
        {
            GameManager.instance.CurrentRerollCount += bonusRerollCount;
            events.Add(new ScoreEventData(ScoreEventData.Type.ItemEffect, itemIndex, totalScore)
            {
                effectName = itemName,
                effectDesc = $"ÁÖ»çÀ§ Àç±¼¸² È½¼ö +{bonusRerollCount}"
            });
            events.Add(new ScoreEventData(ScoreEventData.Type.GainReroll, -1, totalScore));
        }
    }
}
