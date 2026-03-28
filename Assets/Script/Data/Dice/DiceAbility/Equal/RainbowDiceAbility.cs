using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/rainbow")]
public class RainbowDiceAbility : DiceData
{
    public int bonusScore = 3;
    

    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        bool isAllSame = true;
        int firstVal = allDice[0].modifiedValue;
        foreach(var dice in allDice)
        {
            if(dice.modifiedValue != firstVal)
            {
                isAllSame = false;
                break;
            }
        }
        if (isAllSame)
        {
            totalScore *= bonusScore;
            events.Add(new ScoreEventData(ScoreEventData.Type.GlobalBuff, -1, totalScore, "Rainbow"));
        }
    }

}