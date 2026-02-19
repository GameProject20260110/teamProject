using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Sword")]
public class Sword : ItemSo
{
    public int bonusScore = 15;

    private void OnEnable()
    {
        isConsumable = true;
    }
    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        totalScore += bonusScore;

        // 연출 추가
    }
}
