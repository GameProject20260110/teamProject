using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class RoundData
{
    public int roundNum;
    //public int targetScore;
    public int successGoldReward;
    public int failGoldReward;
    //public Sprite enemyImage;
    public bool hasGimmick;
    public EnemyData enemyData;
}

[System.Serializable]
public class GimmickEnemyImage
{
    public string groupName;
    public GimmickType[] gimmickTypes;
    //public Sprite enemyImage;
    public EnemyData enemyData;
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

    public int GetGoldRewardForSuccess(int round)
    {
        RoundData data = GetRoundData(round);
        if (data != null && data.successGoldReward > 0) return data.successGoldReward;

        if (round <= 5) return 10;
        else if (round <= 10) return 12;
        else return 14;
    }

    public int GetGoldRewardForFailure(int round)
    {
        RoundData data = GetRoundData(round);
        if (data != null && data.failGoldReward > 0) return data.failGoldReward;

        if (round <= 4) return 3;
        else if (round <= 9) return 4;
        else return 5;
    }

    public EnemyData GetEnemyDataByGimmick(GimmickType type)
    {
        foreach(var group in gimmickEnemyImages)
        {
            if(group.gimmickTypes != null && group.gimmickTypes.Contains(type))
            {
                return group.enemyData;
            }
        }
        return null;
    }

    [ContextMenu("라운드 기본값 생성")]
    public void GeneratorRoundDefaultRounds()
    {
        allRounds.Clear();

        //int[] targetScores = new int[]
        //{
        //    23, 26, 32, 35, 50, 55, 60, 90, 105, 120, 150, 300, 350, 400, 500
        //};
        int[] successGoldReward = new int[]
        {
            10, 10, 10, 10, 10,
            12, 12, 12, 12, 12,
            14, 14, 14, 14, 14
        };
        int[] failGoldReward = new int[]
        {
            3, 3, 3, 3,
            4, 4, 4 ,4, 4,
            5, 5, 5, 5, 5, 5
        };

        for(int i = 0; i < 15; i++)
        {
            int roundNum = i + 1;
            bool isBoss = (roundNum % 5 == 0);

            allRounds.Add(new RoundData
            {
                roundNum = roundNum,
                //targetScore = targetScores[i],
                successGoldReward = successGoldReward[i],
                failGoldReward = failGoldReward[i],
                hasGimmick = isBoss
            });
        }
    }
}
