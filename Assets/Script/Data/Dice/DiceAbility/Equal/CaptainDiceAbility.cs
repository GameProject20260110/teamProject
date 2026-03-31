using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/captain")]
public class CaptainDiceAbility : DiceData
{
    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        if (myState.diceData != this) return;

        List<int> matchInDices = new List<int>();
        foreach (var dice in allDice)
        {
            if(dice.modifiedValue == myState.modifiedValue)
            {
                matchInDices.Add(dice.diceIndex);
            }
        }
        if (matchInDices.Count <= 1) return;

        totalScore *= matchInDices.Count;
        events.Add(new ScoreEventData(ScoreEventData.Type.TargetBuff, matchInDices.ToArray(), totalScore, $"x{matchInDices.Count}") { effectName = abilityName, effectDesc = "������ ���� �ֻ��� �� * ���� ����" });
    }
}
