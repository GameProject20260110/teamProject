using UnityEngine;
using System.Collections.Generic;

public enum RewardType
{
    Gold,
    Dice,
    HpPotion,
    PassiveItem,
    ActiveItem
}

[System.Serializable]
public class RewardData
{
    public RewardType rewardType;
    public int weight;
    public Sprite icon;

    public DiceData dice;
    public int goldAmount;
    public int healAmount;
    public BattleItemSo item;
}

[CreateAssetMenu(fileName = "RewardDataSo", menuName = "Battle/RewardDataSo")]
public class RewardDataSo : ScriptableObject
{
    [Header("라운드 종료 후 골드 보상")]
    public int clearGold;

    [Header("선택 보상")]
    public List<RewardData> rewardPool;
    public int rewardCount = 3;

    public RewardData GetRandomReward(List<RewardData> exclude = null)
    {
        List<RewardData> candidates = new List<RewardData>();

        foreach(var reward in rewardPool)
        {
            if (exclude != null && exclude.Contains(reward)) continue;
            candidates.Add(reward);
        }

        if (candidates.Count == 0) return null;

        int totalWeight = 0;
        foreach (var reward in candidates)
            totalWeight += reward.weight;

        int random = Random.Range(0, totalWeight);
        int current = 0;

        foreach(var reward in candidates)
        {
            current += reward.weight;
            if (random < current)
                return reward;
        }

        return null;
    }

    public List<RewardData> GetRewards(int count)
    {
        List<RewardData> result = new List<RewardData>();

        for(int i = 0; i < count; i++)
        {
            RewardData reward = GetRandomReward(result);
            if (reward != null)
                result.Add(reward);
        }
        return result;
    }
}


