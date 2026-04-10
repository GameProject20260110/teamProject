using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Linq;

[System.Serializable]
public class RoundData
{
    public int roundNum;
    public int targetScore;
    public int goldReward;
    public Sprite enemyImage;
    public bool bossStage;

    public bool hasGimmick;
    public string gimmickName;
}

[System.Serializable]
public class GimmickEnemyImage
{
    public string groupName;
    public GimmickType[] gimmickTypes;
    public Sprite enemyImage;
}

[CreateAssetMenu(fileName = "NewStageData", menuName = "Stage/StageData")]
public class StageDataSo : ScriptableObject
{
    public List<RoundData> allRounds = new List<RoundData>();
    public List<GimmickEnemyImage> gimmickEnemyImages = new List<GimmickEnemyImage>();

    public RoundData GetRoundData(int round)
    {
        return allRounds.Find(x => x.roundNum == round);
    }

    public int GetGoldReward(int round)
    {
        RoundData data = GetRoundData(round);
        if (data != null && data.goldReward > 0) return data.goldReward;
        return round + 9;
    }

    public Sprite GetEnemyImageByGimmick(GimmickType type)
    {
        foreach(var group in gimmickEnemyImages)
        {
            if(group.gimmickTypes != null && group.gimmickTypes.Contains(type))
            {
                return group.enemyImage;
            }
        }
        return null;
    }

    [ContextMenu("라운드 기본값 생성")]
    public void GeneratorRoundDefaultRounds()
    {
        allRounds.Clear();

        int[] targetScores = new int[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
        };
        int[] goldReward = new int[]
        {
            10, 11, 12, 13, 14, 15, 16, 17, 18 ,19, 20, 21, 22, 23, 24
        };

        for(int i = 0; i < 15; i++)
        {
            int roundNum = i + 1;
            bool isBoss = (roundNum % 5 == 0);

            allRounds.Add(new RoundData
            {
                roundNum = roundNum,
                targetScore = targetScores[i],
                goldReward = goldReward[i],
                bossStage = isBoss,
                hasGimmick = isBoss
            });
        }
    }
}
