using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/bouncer")]
public class BouncerDiceAbility : DiceData
{
    public int plusScore = 10;
    public int bonusScore = 2;
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        
        
        totalScore += plusScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, myState.diceIndex, totalScore, $"+{plusScore}")
        {
            effectName = abilityName,
            effectDesc = $"라운드 점수 +{plusScore} 이후 x{bonusScore}"
        });
        totalScore *= bonusScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, myState.diceIndex, totalScore, $"x{bonusScore}"));
        Bow.TryTrigger(ref totalScore, events);
    }
}
