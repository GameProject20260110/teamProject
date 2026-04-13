using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/witch")]
public class WitchDiceAbility : DiceData
{
    public int bonusScore = 3;
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        if (myState == null) return;
        totalScore *= bonusScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.GlobalBuff, -1, totalScore, $"x{bonusScore}")
        {
            effectName = abilityName,
            effectDesc = this.effectDesc
        });

        int startIndex = myState.diceIndex + 1;

        if(startIndex < allDice.Count)
        {
            int randomTargetIndex = Random.Range(startIndex, allDice.Count);
            allDice[randomTargetIndex].isIgnored = true;
            events.Add(new ScoreEventData(ScoreEventData.Type.Negate, randomTargetIndex, totalScore, "¹«È¿È­"));
        }
        Bow.TryTrigger(ref totalScore, events);
    }

}
