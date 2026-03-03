using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class RoundData
{
    public int roundNum;
    public int targetScore;

    public Sprite enemyImage;
    public bool bossStage;

    public bool hasGimmick;
    public string gimmickName;
}
[CreateAssetMenu(fileName = "NewStageData", menuName = "Stage/StageData")]
public class StageDataSo : ScriptableObject
{
    public List<RoundData> allRounds = new List<RoundData>();

    public RoundData GetRoundData(int round)
    {
        return allRounds.Find(x => x.roundNum == round);
    }
}
