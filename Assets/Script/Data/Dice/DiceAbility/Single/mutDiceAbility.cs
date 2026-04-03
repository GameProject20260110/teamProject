using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/mut")]
public class MutDiceAbility : DiceData
{
    public DiceGachaTable gacha;
    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        myState.diceData = gacha.Roll();
        events.Add(new ScoreEventData(ScoreEventData.Type.Notice, myState.diceIndex, 0, "")
        {
            effectName = abilityName,
            effectDesc = "무작위 주사위 효과로 변경"
        });
    }
}
