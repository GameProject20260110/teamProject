using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/seller")]
public class SellerDiceAbility : DiceData
{
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        if (myState == null) return;
        int gold = PlayerManager.instance != null ? PlayerManager.instance.gold : 0;
        totalScore += gold;
        events.Add(new ScoreEventData(ScoreEventData.Type.TriggerDice, myState.diceIndex)
        {
            effectName = abilityName,
            effectDesc = this.effectDesc
        });
        events.Add(new ScoreEventData(ScoreEventData.Type.Notice, myState.diceIndex, totalScore, $"+{gold}", myState.scoreValue));
        Bow.TryTrigger(ref totalScore, events);
    }
}