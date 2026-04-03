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
        events.Add(new ScoreEventData(ScoreEventData.Type.GainGold, myState.diceIndex, gold, $"Gold +{gold}")
        {
            effectName = abilityName,
            effectDesc = "¶ó¿îµå ¼ö¸¸Å­ °ñµå È¹µæ"
        });
        Bow.TryTrigger(ref totalScore, events);
    }
}
