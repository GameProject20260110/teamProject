using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "Ability", menuName = "ItemAbility/Bead")]
public class Bead : ItemSo
{
    public int minBonus = 5;
    public int mnxBonus = 15;

    private void OnEnable()
    {
        isConsumable = true;
    }
    public override void RoundStart(List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
    {
        int randomBonusScore = Random.Range(minBonus, mnxBonus + 1);
        totalScore += randomBonusScore;
    }
}
