using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/wizard")]
public class WizardDiceAbility : DiceData
{
    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, List<ScoreEventData> events)
    {
        myState.isForceOdd = true;
    }
}
