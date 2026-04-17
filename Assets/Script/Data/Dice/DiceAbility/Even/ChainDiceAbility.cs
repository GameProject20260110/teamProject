using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/chain")]
public class ChainDiceAbility : DiceData
{
    public int bonusScore = 6;
    public int count = 3;

    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        List<DiceState> targets = new List<DiceState>(allDice);
        int loopCount = Mathf.Min(count, targets.Count);

        events.Add(new ScoreEventData(ScoreEventData.Type.TriggerDice, myState.diceIndex)
        {
            effectName = abilityName,
            effectDesc = this.effectDesc
        });

        for(int i = 0; i < loopCount; i++)
        {
            int rand = Random.Range(i, targets.Count);
            DiceState target = targets[rand];
            targets[rand] = targets[i];
            targets[i] = target;

            target.modifiedValue = 6;
            target.scoreValue = bonusScore;

            int diff = target.ApplyDiceScoreChange(bonusScore);
            if (diff != 0) totalScore += diff;

            events.Add(new ScoreEventData(ScoreEventData.Type.ChangeFace, target.diceIndex, totalScore, "Chain!", bonusScore));
        }
        Bow.TryTrigger(ref totalScore, events);
        ChangeModi(myState, allDice, ref totalScore, events);
    }
}
