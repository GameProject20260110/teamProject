using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/mut")]
public class MutDiceAbility : DiceData
{
    public DiceGachaTable gacha;
    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, List<ScoreEventData> scoreEvnet)
    {
        myState.diceData = gacha.Roll();
    }
}
