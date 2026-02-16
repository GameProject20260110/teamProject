using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/cutter")]
public class CutterDiceAbility : DiceData
{
    
    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, List<ScoreEventData> events)
    {
        int count = 2;
        int usedNum = -1;
        int randNum = -1;
        while(count > 0)
        {
            randNum = Random.Range(0,allDice.Count - 1);
            if (randNum == usedNum) continue;

            usedNum = randNum;
            allDice[randNum].isMulti = true;
            Debug.Log(allDice[randNum].diceData.diceNum);
            count--;
        }
        
    }

}
