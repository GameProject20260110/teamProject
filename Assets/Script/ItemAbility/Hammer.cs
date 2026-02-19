using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Hammer")]
public class Hammer : ItemSo
{
    public int bonusScore = 1;
    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
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
            List<DiceState> sameDiceList = new List<DiceState>();

            if(sameDiceList.Count >= 2)
            {
                foreach(var dice in sameDiceList)
                {
                    dice.scoreValue += bonusScore;
                }
            }
        }
    }
}
