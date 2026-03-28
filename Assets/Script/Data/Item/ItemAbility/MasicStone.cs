using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/MasicStone")]
public class MasicStone : ItemSo
{
    public int bonusScore = 2;
    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        totalScore *= bonusScore;
        events.Add(new ScoreEventData(ScoreEventData.Type.ItemEffect, -1, totalScore, $"x{bonusScore}") { effectName = "마석", effectDesc = "라운드 점수 x2\n이후 발동한 효과"});
        var candidate = allDice.FindAll(d => d != null && !d.isIgnored && d.diceData.type != ScoreManager.DiceType.None);
        if(candidate.Count > 0)
        {
            DiceState target = candidate[Random.Range(0, candidate.Count)];
            target.isIgnored = true;
            events.Add(new ScoreEventData(ScoreEventData.Type.Negate, target.diceIndex, 0, "무효화"));
        }
        
    }
}
