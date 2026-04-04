using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.SocialPlatforms.Impl;

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
            if (uiDice[i] == null) continue;
            if (!uiDice[i].gameObject.activeInHierarchy) continue;
            if (uiDice[i] != null && uiDice[i].MyState != null && uiDice[i].MyState.diceData != null)
            {
                DiceData data = uiDice[i].MyState.diceData;
                if(filterType != DiceType.Roll && data.type != filterType)
                {
                    continue;
                }
                simulationStates.Add(new DiceState(data, i, uiDice[i].MyState.originalValue));
            }
        }

        bool gimmickNoScoreNormal = IsGimmickActive(GimmickType.NoScoreFromNormalDice);
        bool gimmickNegateItem = IsGimmickActive(GimmickType.NegateRandomItem);
        bool gimmickNegateDiceEffect = IsGimmickActive(GimmickType.NegateRandomDiceEffect);

        if(gimmickNegateDiceEffect)
        {
            var candidates = simulationStates.FindAll(s => s != null && !s.isIgnored && s.diceData.type != DiceType.None);

            if(candidates.Count > 0)
            {
                DiceState target = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                target.isIgnored = true;
                // 기믹 연출 이벤트 추가 필요
                Debug.Log($"{target.diceIndex} 효과 무효화!");
            }
        }

        // 0단계
        foreach (var state in simulationStates)
        {
            if (state == null || state.isIgnored) continue;

            if(gimmickNoScoreNormal && state.diceData.type == DiceType.None)
            {
                // 기믹 연출 이벤트 추가 필요
                Debug.Log("효과 없는 주사위 점수 획득 불가!");
                continue;
            }

            finalScore += state.originalValue;
            state.appliedScoreValue = state.originalValue;
            scoreEvents.Add(new ScoreEventData(ScoreEventData.Type.AddScore, state.diceIndex, finalScore, $"+{state.originalValue}", state.originalValue));
        }


        // 아이템 효과
        List<ItemSo> playerInventory = GetPlayerInventory();
        if(playerInventory != null /*&& PlayerManager.instance != null*/)
        {
            for (int i = 0; i < playerInventory.Count; i++)
            {
                var item = playerInventory[i];  
                if (item == null) continue;

                if (gimmickNegateItem && Random.value < 0.25f)
                { 
                    continue;
                }
                if (itemsToComsume.Contains(item)) continue;
                item.RoundStart(simulationStates, ref finalScore, scoreEvents, i); 
                Debug.Log($"RoundStart 호출: {item.itemName}");

                if (item.isConsumable)
                {
                    itemsToComsume.Add(item);
                }
            }

        }

        if(PlayerShopManager.instance != null)
        {
            var pending = PlayerShopManager.instance.pendingConsumables;
            for(int i = 0; i < pending.Count; i++)
            {
                var item = pending[i];
                if (item == null) continue;
                item.RoundStart(simulationStates, ref finalScore, scoreEvents, -1);
            }
        }

        // 점수 로직
        // 1. 룰상 효과
        foreach (var state in simulationStates)
        {
            state.diceData.OnRuleEffect(state, simulationStates, scoreEvents);
        }

        // 2. 굴림 효과
        foreach (var state in simulationStates)
        {

            if (state == null || state.isIgnored) continue;
            state.diceData.OnRollEffect(state, simulationStates, ref finalScore, scoreEvents);
        }

        // 3. 계산 시/중 효과

        foreach (var state in simulationStates)
        {
            if(state == null || state.isIgnored)
            {
                continue;
            }

            state.diceData.CalculateEffect(state, simulationStates, ref finalScore, scoreEvents);
        }

        // 4. 점수 계산 후 효과
        foreach (var state in simulationStates)
        {
            if (state == null || state.isIgnored) continue;
            if (state.diceData is CutterDiceAbility) continue;
            state.diceData.AfterCalculateEffect(state, simulationStates, ref finalScore, scoreEvents);
        }

        foreach(var state in simulationStates)
        {
            if (state == null || state.isIgnored) continue;
            if (state.diceData is CutterDiceAbility)
            {
                state.diceData.AfterCalculateEffect(state, simulationStates, ref finalScore, scoreEvents);
            }
        }

        scoreEvents.Add(new ScoreEventData(
            ScoreEventData.Type.FinalScore, 
            -1,
            finalScore,
            "Total"));

        return (finalScore, scoreEvents, itemsToComsume);
    }

    private bool IsGimmickActive(GimmickType type)
    {
        return GimmickManager.instance != null && GimmickManager.instance.IsGimmickActive(type);
    }

    private List<ItemSo> GetPlayerInventory()
    {
        if(TestModeManager.instance != null && TestModeManager.instance.isTestModeActive)
        {
            return TestModeManager.instance.testItem;
        }
        if (PlayerManager.instance != null) return PlayerManager.instance.items;

        return null;
    }
}


