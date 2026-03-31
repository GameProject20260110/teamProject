using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DiceData", menuName = "Scriptable Objects/DiceData")]
public class DiceData : ScriptableObject
{
    public enum DiceTiming { Rule, Roll, Calculate, After}

    public int multiBonusScore = 1;
    public int plusBonusScore = 0;

    [Header("주사위 기본 정보")]
    public ScoreManager.DiceType type;
    public DiceTiming timing;
    public int diceNum;
    public string abilityName;

    [Range(1, 5)]
    public int tier = 1;

    public int gold;
    public int sell;
    
    [TextArea]
    public string Desc;

    [Header("주사위 스킨")]
    public DiceSkin skin;

    // myState: 내 주사위 상태, allDice: 모든 주사위 상태 리스트
    public void ChangeModi(DiceState myState, List<DiceState> allDice, List<ScoreEventData> scoreEvent) {
        if (myState.changeValue == 0) return;
        foreach (var dice in allDice)
        {
            if (!dice.change) continue;
            dice.scoreValue += dice.changeValue;
            scoreEvent.Add(new ScoreEventData(ScoreEventData.Type.AddScore, dice.diceIndex, 0, $"Mono +{dice.changeValue}"));
        }
    }

    public virtual void OnRuleEffect(DiceState myState, List<DiceState> allDice, List<ScoreEventData> scoreEvent) { }

    public virtual void OnRollEffect(DiceState myState, List<DiceState> allDice, ref int score, List<ScoreEventData> scoreEvent) { }

    public virtual void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int score, List<ScoreEventData> scoreEvent) { }

    public virtual void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int score, List<ScoreEventData> scoreEvent) { }
}