using UnityEngine;

public class ScoreEventData
{
    public enum Type
    {
        AddScore,      
        Multiplier,    
        TargetBuff,    
        ChangeFace,    
        GlobalBuff,   
        Negate,         
        ItemEffect,     
        FinalScore,     
        GainGold,
        GainReroll,
        Notice
    }

    public Type type; 
    public int targetIndex; 
    public int[] targetIndices;
    public int value;          
    public string desc;        
    public string effectName;
    public string effectDesc;  
    public int currentDiceScore;

    public ScoreEventData(Type type, int targetIndex, int value, string desc = "", int currentDiceScore = int.MinValue)
    {
        this.type = type;
        this.targetIndex = targetIndex;
        this.value = value;
        this.desc = desc;
        this.currentDiceScore = currentDiceScore;
    }

    public ScoreEventData(Type type, int[] targetIndices, int value, string desc = "", int currentDiceScore = int.MinValue)
    {
        this.type = type;
        this.targetIndices = targetIndices;
        this.targetIndex = -1;
        this.value = value;
        this.desc = desc;
        this.currentDiceScore = currentDiceScore;
    }
}
