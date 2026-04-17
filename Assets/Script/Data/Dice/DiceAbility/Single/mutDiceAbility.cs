using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/mut")]
public class MutDiceAbility : DiceData
{
    public DiceGachaTable gacha;
    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        DiceData newData = gacha.Roll();
        myState.diceData = newData;

        events.Add(new ScoreEventData(ScoreEventData.Type.Notice, myState.diceIndex, 0, "")
        {
            effectName = abilityName,
            effectDesc = this.effectDesc
        });

        events.Add(new ScoreEventData(ScoreEventData.Type.Notice, myState.diceIndex, totalScore)
        {
            effectName = newData.abilityName,
            
        });
        if(!(newData is MutDiceAbility)) newData.OnRollEffect(myState, allDice, ref totalScore, events);
    }
}
