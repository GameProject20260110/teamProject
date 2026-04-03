using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Bow")]
public class Bow : ItemSo
{
    public int bonusRoundScore = 1;
    public static void TryTrigger(ref int totalScore, List<ScoreEventData> events)
    {
        List<ItemSo> items;
        if(TestModeManager.instance != null && TestModeManager.instance.isTestModeActive)
        {
            items = TestModeManager.instance.testItem;
        }
        else
        {
            items = PlayerManager.instance.items;
        }
        if (items == null) return;

        foreach(var item in items)
        {
            if(item is Bow bow)
            {
                totalScore += bow.bonusRoundScore;
                events.Add(new ScoreEventData(ScoreEventData.Type.ItemEffect, -1, totalScore)
                {
                    effectName = bow.itemName,
                    effectDesc = $"주사위 효과 발동 시 + {bow.bonusRoundScore}점"
                });
            }
        }
    }
}
