using UnityEngine;

public class ScoreEventData
{
    public enum Type
    {
        AddScore,      // 점수 추가
        Multiplier,    // 점수 배율
        TargetBuff,    // 대상 지정
        ChangeFace,    // 눈금 변경
        GlobalBuff,   // 전체 효과
        Negate,         // 무효화
        ItemEffect,     // 아이템
        FinalScore,     // 최종 점수
        GainGold
    }

    public Type type; 
    public int targetIndex; // 연출 대상
    public int[] targetIndices;
    public int value;          // 변동된 값
    public string desc;        // floatingMessage 용
    public string effectName;
    public string effectDesc;  // 효과 적용 메세지용
    public int currentDiceScore;

    public ScoreEventData(Type type, int targetIndex, int value, string desc, int currentDiceScore = -1)
    {
        this.type = type;
        this.targetIndex = targetIndex;
        this.value = value;
        this.desc = desc;
        this.currentDiceScore = currentDiceScore;
    }

    public ScoreEventData(Type type, int[] targetIndices, int value, string desc, int currentDiceScore = -1)
    {
        this.type = type;
        this.targetIndices = targetIndices;
        this.targetIndex = -1;
        this.value = value;
        this.desc = desc;
        this.currentDiceScore = currentDiceScore;
    }
}
