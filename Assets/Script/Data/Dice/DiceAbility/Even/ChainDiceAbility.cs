using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/chain")]
public class ChainDiceAbility : DiceData
{
    public int bonusScore = 6;
    public int count = 3;

    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, List<ScoreEventData> events)
    {
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, allDice.Count);
            allDice[randomIndex].scoreValue = bonusScore;
        }

        List<DiceState> targets = new List<DiceState>(allDice);
        int loopCount = Mathf.Min(count, targets.Count);

        for(int i = 0; i < loopCount; i++)
        {
            int rand = Random.Range(i, targets.Count);
            DiceState target = targets[rand];
            targets[rand] = targets[i];
            targets[i] = target;

            target.modifiedValue = 6;
            target.scoreValue = bonusScore;

            // ¿¬Ãâ
            events.Add(new ScoreEventData(ScoreEventData.Type.ChangeFace, target.diceIndex, 6, "Chain!"));

        }
    }

}
