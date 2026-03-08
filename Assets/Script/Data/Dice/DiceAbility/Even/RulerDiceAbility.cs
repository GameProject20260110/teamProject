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
        int bonus = 0;
        foreach(var dice in allDice)
        {
            if (dice == null) continue;

            if(dice.currentType == ScoreManager.DiceType.Even)
            {
                bonus += dice.scoreValue;
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, 0, "Ruler"));
            }

            if(bonus > 0)
            {
                totalScore += bonus;
                events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, myState.diceIndex, totalScore, $"+{bonus} (Ruler)"));
            }
        }
    }
}
