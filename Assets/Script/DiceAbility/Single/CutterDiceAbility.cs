using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/cutter")]
public class CutterDiceAbility : DiceData
{

    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        List<DiceState> others = allDice.FindAll(d => d != myState);

        if (others.Count == 0) return;

        int randNum = Random.Range(0, others.Count);
        int add = myState.scoreValue * others[randNum].modifiedValue;
        totalScore += add;
        events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, others[randNum].diceIndex, totalScore, $"Cutter"));
        events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, myState.diceIndex, totalScore, $"Cutter +{add}"));
    }

}
