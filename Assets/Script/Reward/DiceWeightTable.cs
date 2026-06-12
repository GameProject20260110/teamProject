using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DiceLevel
{
    public int level;
    public int weight;
    public List<DiceData> dices;
}

[CreateAssetMenu(fileName = "DiceWeightTable", menuName = "Reward/DiceWeightTable")]
public class DiceWeightTable : ScriptableObject
{
    public List<DiceLevel> diceLevels;

    public DiceData GetRandomDice()
    {
        if (diceLevels == null || diceLevels.Count == 0) return null;

        // 1단계 가중치 계산
        int totalWeight = 0;
        foreach (var level in diceLevels)
            totalWeight += level.weight;

        int random = Random.Range(0, totalWeight);
        int current = 0;
        DiceLevel selectedLevel = null;

        foreach(var level in diceLevels)
        {
            current += level.weight;
            if(random < current)
            {
                selectedLevel = level;
                break;
            }
        }

        if(selectedLevel == null || selectedLevel.dices == null || selectedLevel.dices.Count == 0)
            return null;

        // 2단계 : 해당 레벨에서 주사위 랜덤 선택
        return selectedLevel.dices[Random.Range(0, selectedLevel.dices.Count)];
    }
}
