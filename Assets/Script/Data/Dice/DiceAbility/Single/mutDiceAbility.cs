using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/mut")]
public class MutDiceAbility : DiceData
{
    public DiceGachaTable gacha;
    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {

        DiceData newData;
        do
        {
            newData = gacha.Roll();
        } while (newData is MutDiceAbility);
        myState.diceData = newData;

        events.Add(new ScoreEventData(ScoreEventData.Type.TriggerDice, myState.diceIndex)
        {
            effectName = abilityName,
            effectDesc = this.effectDesc
        });

        events.Add(new ScoreEventData(ScoreEventData.Type.Notice, myState.diceIndex, totalScore)
        {
            effectName = newData.abilityName,
            effectDesc = newData.effectDesc
        });
    }
}
