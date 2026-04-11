using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CreateAssetMenu(fileName = "DiceGachaTable", menuName = "Gacha/DiceGachaTable")]
public class DiceGachaTable : ScriptableObject
{
    public int level;

    [System.Serializable]
    public class DiceWeight
    {
        public DiceData dice;
        public int weight;
    }

    public List<DiceWeight> diceWeights;

    public DiceData Roll()
    {
        int total = 0;
        int weight = 0;
        int randNum = 0;

        foreach (var dices in diceWeights)
        {
            total += dices.weight;
        }
        randNum = Mathf.RoundToInt(total * Random.Range(0.0f, 1.0f));
        foreach (var dices in diceWeights)
        {
            weight += dices.weight;
            if (weight >= randNum)
            {
                return dices.dice;
            }
        }
        return null;
    }

    [ContextMenu("Tier별 가격 기반 확률로 넣기 (추천)")]
    private void AutoFillDiceWeightsByTierAndPrice()
    {
        // 1. 모든 DiceData 찾기
        string[] guids = AssetDatabase.FindAssets("t:DiceData");
        List<DiceData> allDice = new List<DiceData>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DiceData dice = AssetDatabase.LoadAssetAtPath<DiceData>(path);
            if (dice != null && dice.type != ScoreManager.DiceType.None)
            {
                allDice.Add(dice);
            }
        }

        // 2. 리스트 초기화
        diceWeights = new List<DiceWeight>();

        // 3. Tier별 기본 weight
        foreach (var dice in allDice)
        {
            int weight = CalculateWeightByTier(dice.tier);

            diceWeights.Add(new DiceWeight
            {
                dice = dice,
                weight = weight
            });
        }

        // 4. 정렬
        diceWeights = diceWeights
            .OrderBy(d => d.dice.diceNum)
            .ToList();

        // 5. 저장
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

    }


    private int CalculateWeightByTier(int tier)
    {
        return GetBaseWeightByTier(tier);
    }

    private int GetBaseWeightByTier(int tier)
    {
        switch (level)
        {
            case 1:
                return tier switch
                {
                    1 => 62,
                    2 => 59,
                    3 => 30,
                    4 => 4,
                    5 => 2,
                    _ => 0
                };

            case 2:
                return tier switch
                {
                    1 => 40,
                    2 => 55,
                    3 => 40,
                    4 => 25,
                    5 => 4,
                    _ => 0
                };

            case 3:
                return tier switch
                {
                    1 => 25,
                    2 => 47,
                    3 => 45,
                    4 => 35,
                    5 => 15,
                    _ => 0
                };

            case 4:
                return tier switch
                {
                    1 => 20,
                    2 => 35,
                    3 => 40,
                    4 => 46,
                    5 => 30,
                    _ => 0
                };

            case 5:
                return tier switch
                {
                    1 => 15,
                    2 => 25,
                    3 => 30,
                    4 => 55,
                    5 => 50,
                    _ => 0
                };

            default:
                return tier switch
                {
                    1 => 62,
                    2 => 59,
                    3 => 30,
                    4 => 4,
                    5 => 2,
                    _ => 0
                };
        }
    }
}


