using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/bouncer")]
public class BouncerDiceAbility : DiceData
{
    public int plusScore = 10;
    public int bonusScore = 2;
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        events.Add(new ScoreEventData(ScoreEventData.Type.TriggerDice, myState.diceIndex)
        {
            effectName = abilityName,
            effectDesc = this.effectDesc
        });

        totalScore += plusScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, myState.diceIndex, totalScore, $"+{plusScore}"));
        totalScore *= bonusScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, myState.diceIndex, totalScore, $"x{bonusScore}"));
        Bow.TryTrigger(ref totalScore, events);
    }
}
