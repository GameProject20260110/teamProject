using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Hammer")]
public class Hammer : ItemSo
{
    public int bonusScore = 1;
    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events, int itemIndex = -1)
    {
        Dictionary<int, List<DiceState>> valueGroups = new Dictionary<int, List<DiceState>>();

        foreach(var dice in allDice)
        {
            if (dice == null || dice.isIgnored) continue;

            int val = dice.originalValue;

            if(!valueGroups.ContainsKey(val))
            {
                valueGroups[val] = new List<DiceState>();
            }
            valueGroups[val].Add(dice);
        }

        foreach(var group in valueGroups)
        {
            List<DiceState> sameDiceList = group.Value;
            if (sameDiceList.Count < 2) continue;

            List<int> targetIndice = new List<int>();
            foreach (var dice in sameDiceList)
            {
                int score = dice.scoreValue + bonusScore;
                int diff = dice.ApplyDiceScoreChange(score);
                if(diff != 0)
                {
                    totalScore += diff;
                    targetIndice.Add(dice.diceIndex);
                }
            }

            if(targetIndice.Count > 0)
            {
                events.Add(new ScoreEventData(ScoreEventData.Type.ItemEffect, itemIndex, totalScore)
                {
                    effectName = itemName,
                    effectDesc = $"모든 동일한 눈금 점수 + {bonusScore}점"
                });
            }
        }
    }
}
