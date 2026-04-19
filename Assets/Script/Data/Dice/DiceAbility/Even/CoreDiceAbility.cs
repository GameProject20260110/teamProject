using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/core")]
public class CoreDiceAbility : DiceData
{

    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        foreach(var dice in allDice)
        {
            if(dice != null && dice.diceIndex < myState.diceIndex && dice.currentType == ScoreManager.DiceType.Even)
            {
                if (dice.diceData is CoreDiceAbility) continue;
                if (dice.diceData is RulerDiceAbility) continue;

                events.Add(new ScoreEventData(ScoreEventData.Type.TriggerDice, myState.diceIndex)
                {
                    effectName = abilityName,
                    effectDesc = this.effectDesc
                });

                dice.diceData.CalculateEffect(dice, allDice, ref totalScore, events);
                dice.diceData.AfterCalculateEffect(dice, allDice, ref totalScore, events);
            }
        }
    }
}
