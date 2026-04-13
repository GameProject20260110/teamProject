using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/cutter")]
public class CutterDiceAbility : DiceData
{

    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        List<DiceState> targets = new List<DiceState>(allDice);
        int rand1 = Random.Range(0, targets.Count);
        DiceState dice1 = targets[rand1];
        targets.RemoveAt(rand1);
        int rand2 = Random.Range(0, targets.Count);
        DiceState dice2 = targets[rand2];

        int removedScore = dice1.scoreValue + dice2.scoreValue;
        int multiScore = dice1.scoreValue * dice2.scoreValue;
        int diff = multiScore - removedScore;

        totalScore += diff;

        events.Add(new ScoreEventData(ScoreEventData.Type.TargetBuff, new int[] {dice1.diceIndex, dice2.diceIndex}, totalScore)
        {
            effectName = abilityName,
            effectDesc = this.effectDesc
        });
        Bow.TryTrigger(ref totalScore, events);
    }

}
