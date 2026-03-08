using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Wand")]
public class Wand : ItemSo
{
    public int bonusScore = 2;
    public override void RoundStart(List<DiceState> allDice, ref int totalSoce, List<ScoreEventData> events)
    {
        foreach (var dice in allDice)
        {
            if(dice.diceData.diceNum == 0 && dice.diceData.type == ScoreManager.DiceType.None)
            {
                dice.scoreValue += bonusScore;
            }
        }
    }
}
