using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/mono")]
public class MonoDiceAbility : DiceData
{

    public override void OnRuleEffect(DiceState myState, List<DiceState> allDice, List<ScoreEventData> events)
    {
        foreach (var dice in allDice)
        {
            dice.changeValue += 3;
            events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, 0, "Mono"));
        }
    }

}
