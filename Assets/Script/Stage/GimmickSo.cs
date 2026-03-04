using UnityEngine;

public enum GimmickType
{
    None = 0,
    NoScoreFromNormalDice = 1,
    NoReroll = 2,
    LoseRandomItem = 3,
    NegateRandomItem = 4,
    NegateRandomDiceEffect = 5,
    LoseGold = 6,
    DoubleTargetScore = 7,
    LoseHeartDice = 8
}

public enum  GimmickCategory
{
    Negate,
    Block,
    AfterEffect
}

public abstract class GimmickSo : ScriptableObject
{
    [Header("±â¹Í Á¤º¸")]
    public string gimmickName;
    public string description;
    public int level;
    public GimmickType gimmickType;
    public GimmickCategory category;
    public abstract void ExecuteGimmick();
}
