using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/mirror")]
public class MirrorDiceAbility : DiceData
{

    public override void OnRuleEffect(DiceState myState, List<DiceState> allDice, List<ScoreEventData> events)
    {
        if(myState.diceIndex > 0)
        {
            var targetDice = allDice[myState.diceIndex - 1];
            targetDice.diceData.OnRuleEffect(targetDice, allDice, events);
        }
    }

    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, List<ScoreEventData> events)
    {
        if(myState.diceIndex > 0)
        {
            var targetDice = allDice[myState.diceIndex - 1];
            targetDice.diceData.OnRollEffect(targetDice, allDice, events);
        }
    }

    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int score, List<ScoreEventData> events)
    {
        if(myState.diceIndex > 0)
        {
            var targetDice = allDice[myState.diceIndex - 1];
            targetDice.diceData.CalculateEffect(myState, allDice, ref score, events);
        }
    }

    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int score, List<ScoreEventData> events)
    {
        if(myState.diceIndex > 0)
        {
            var targetDice = allDice[myState.diceIndex - 1];
            targetDice.diceData.AfterCalculateEffect(targetDice, allDice, ref score, events);
        }
    }

}
