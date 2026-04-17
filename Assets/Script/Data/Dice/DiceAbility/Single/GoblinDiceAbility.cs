using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/goblin")]
public class GoblinDiceAbility : DiceData
{
    

    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        if(myState == null) return;
        if (PlayerManager.instance == null) return;

        int gold = RoundManager.instance.currentRound;
        PlayerManager.instance.gold += gold;
        events.Add(new ScoreEventData(ScoreEventData.Type.TriggerDice, myState.diceIndex)
        {
            effectName = abilityName,
            effectDesc = this.effectDesc
        });
        events.Add(new ScoreEventData(ScoreEventData.Type.GainGold, myState.diceIndex, gold, $"Gold +{gold}"));
        Bow.TryTrigger(ref totalScore, events);
    }
}
