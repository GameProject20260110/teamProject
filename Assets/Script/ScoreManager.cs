using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public enum DiceType { Even, Odd, Equal, Single, None, Roll }

    private void Awake()
    {
        if (instance == null) instance = this;
    }


    public (int totalScore, List<ScoreEventData> events) CalculateScore(Dice[] uiDice, DiceType filterType = DiceType.Roll)
    {
        List<DiceState> simulationStates = new List<DiceState>();
        List<ScoreEventData> scoreEvents = new List<ScoreEventData>();
        int totalScore = 0;

        for(int i = 0; i < uiDice.Length; i++)
        {
            if (uiDice[i] != null)
            {
                DiceData data = uiDice[i].MyState.diceData;
                if(filterType != DiceType.Roll && data.type != filterType)
                {
                    continue;
                }
                simulationStates.Add(new DiceState(data, i, uiDice[i].MyState.originalValue));
            }
        }



        // 점수 로직
        // 1. 룰상 효과
        foreach (var state in simulationStates)
        {

            //if (state == null || state.isIgnored) continue;
            state.diceData.OnRuleEffect(state, simulationStates, scoreEvents);
        }

        // 2. 굴림 효과
        foreach (var state in simulationStates)
        {

            if (state == null || state.isIgnored) continue;
            state.diceData.OnRollEffect(state, simulationStates, scoreEvents);
        }

        // 3. 계산 시/중 효과
        
        foreach (var state in simulationStates)
        {
            if(state == null || state.isIgnored)
            {
                continue;
            }

            if (state.isMulti)
            {
                totalScore *= state.scoreValue;
                scoreEvents.Add(new ScoreEventData(ScoreEventData.Type.Multiplier, state.diceIndex, totalScore, $"x{state.scoreValue}"));
            }
            else
            {
                totalScore += state.scoreValue;
                scoreEvents.Add(new ScoreEventData(ScoreEventData.Type.AddScore, state.diceIndex, totalScore, $"+{state.scoreValue}"));
            }

            
            state.diceData.CalculateEffect(state, simulationStates, ref totalScore, scoreEvents);
        }

        // 4. 점수 계산 후 효과
        foreach (var state in simulationStates)
        {
            if (state == null || state.isIgnored) continue;
            state.diceData.AfterCalculateEffect(state, simulationStates, ref totalScore, scoreEvents);
        }

        scoreEvents.Add(new ScoreEventData(
            ScoreEventData.Type.FinalScore, 
            -1,
            totalScore,
            "Total"));

        return (totalScore, scoreEvents);
    }

}


