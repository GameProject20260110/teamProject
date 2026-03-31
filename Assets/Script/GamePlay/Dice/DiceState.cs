
using JetBrains.Annotations;
using System.Data;
using UnityEditor;
using UnityEngine;

public class DiceState
{
    public DiceData diceData;

    public int diceIndex;      // �ֻ��� ����
    public int originalValue; // ���� �ֻ��� ��
    public int modifiedValue;  // ȿ�� ���� �� �ֻ��� ��
    public int scoreValue;    // ���� ���� �ֻ��� ��
    public int changeValue;     
    public bool change;

    public bool isIgnored = false;
    public bool isMulti = false;
    public int multiBonusScore;
    public int plusBonusScore;

    public int appliedScoreValue;

    public ScoreManager.DiceType currentType;
    public bool isForceOdd = false;
    public bool isForceEven = false;

    public bool IsCurrentEven
    {
        get
        {
            if (isForceEven) return true;
            if (isForceOdd) return false;
            return modifiedValue % 2 == 0;
        }
    }

    public DiceState(DiceData data, int index, int value)
    {
        diceData = data;
        diceIndex = index;
        originalValue = value;
        modifiedValue = value;
        scoreValue = value;
        changeValue = 0;
        change = false;
        isMulti = false;
        isIgnored = false;
        appliedScoreValue = 0;

        if(data != null)
        {
            this.currentType = data.type;
            multiBonusScore = data.multiBonusScore;
            plusBonusScore = data.plusBonusScore;

        }
        else
        {
            this.currentType = ScoreManager.DiceType.None;
            this.multiBonusScore = 1;
            this.plusBonusScore = 0;
        }
        this.isForceOdd = false;
        this.isForceEven = false;
    }

    public int ApplyDiceScoreChange(int diceScore)
    {
        int diff = diceScore - appliedScoreValue;
        scoreValue = diceScore;
        appliedScoreValue = diceScore;
        return diff;
    }
}
