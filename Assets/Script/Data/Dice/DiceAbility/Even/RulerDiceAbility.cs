using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/ruler")]
public class RulerDiceAbility : DiceData
{
    public override void OnRuleEffect(DiceState myState, List<DiceState> allDice, List<ScoreEventData> events)
    {
        myState.currentType = ScoreManager.DiceType.None;
    }

    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        foreach(var dice in allDice)
        {
            if (dice == null || dice == myState) continue;

            if (dice.currentType != ScoreManager.DiceType.Even) continue;

            dice.diceData.CalculateEffect(dice, allDice, ref totalScore, events);
            dice.diceData.AfterCalculateEffect(dice, allDice, ref totalScore, events);
            events.Add(new ScoreEventData(ScoreEventData.Type.GlobalBuffs, dice.diceIndex, totalScore, $"Ruler: {dice.diceData.name} Àç¹ßµ¿"));
        }
    }
}
