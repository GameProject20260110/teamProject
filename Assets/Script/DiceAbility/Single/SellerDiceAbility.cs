using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/seller")]
public class SellerDiceAbility : DiceData
{
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        if (myState == null) return;
        totalScore += PlayerManager.instance.gold;
        events.Add(new ScoreEventData(ScoreEventData.Type.AddScore, myState.diceIndex, totalScore, $"Seller +{PlayerManager.instance.gold}"));
    }

}