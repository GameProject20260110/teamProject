using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    public (int finalScore, List<ScoreEventData> events, List<ItemSo> consumedItems) CalculateScore(Dice[] uiDice, DiceType filterType = DiceType.Roll)
    {
        List<DiceState> simulationStates = new List<DiceState>();
        List<ScoreEventData> scoreEvents = new List<ScoreEventData>();
        List<ItemSo> itemsToComsume = new List<ItemSo>();
        int finalScore = 0;

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

        // 0단계
        foreach(var state in simulationStates)
        {
            if (state == null || state.isIgnored) continue;
            finalScore += state.originalValue;

            scoreEvents.Add(new ScoreEventData(ScoreEventData.Type.AddScore, state.diceIndex, finalScore, $"+{state.originalValue}", state.originalValue));
        }

        if(GameManager.instance != null && GameManager.instance.playerData != null)
        {
            List<ItemSo> inventory = GameManager.instance.playerData.itemSo;

            if(inventory != null)
            {
                foreach(var item in inventory)
                {
                    if (item == null) continue;
                    item.RoundStart(simulationStates, ref finalScore, scoreEvents);

                    if(item.isConsumable)
                    {
                        itemsToComsume.Add(item);
                    }
                }
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

            //if (state.isMulti)
            //{
            //    finalScore *= state.scoreValue;
            //    scoreEvents.Add(new ScoreEventData(ScoreEventData.Type.Multiplier, state.diceIndex, finalScore, $"x{state.scoreValue}"));
            //}
            //else
            //{
            //    finalScore += state.scoreValue;
            //    scoreEvents.Add(new ScoreEventData(ScoreEventData.Type.AddScore, state.diceIndex, finalScore, $"+{state.scoreValue}"));
            //}

            state.diceData.CalculateEffect(state, simulationStates, ref finalScore, scoreEvents);
        }

        // 4. 점수 계산 후 효과
        foreach (var state in simulationStates)
        {
            if (state == null || state.isIgnored) continue;
            state.diceData.AfterCalculateEffect(state, simulationStates, ref finalScore, scoreEvents);
        }

        scoreEvents.Add(new ScoreEventData(
            ScoreEventData.Type.FinalScore, 
            -1,
            finalScore,
            "Total"));

        return (finalScore, scoreEvents, itemsToComsume);
    }

}


