using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DiceGachaTable",menuName = "Gacha/DiceGachaTable")]
public class DiceGachaTable : ScriptableObject
{
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
}
