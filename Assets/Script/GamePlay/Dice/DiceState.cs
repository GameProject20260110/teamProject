
using System.Collections.Generic;
public class DiceState
{
    public DiceData diceData;

    public int diceIndex;      // 주사위 순서
    public int originalValue;  // 최초 주사위 값
    public int modifiedValue;  // 효과 적용 후 주사위 값
    public int scoreValue;     // 점수 계산용 주사위 값
    public int changeValue;     
    public bool change;

    public bool isIgnored = false;
    public bool isMulti = false;
    public int multiBonusScore;
    public int plusBonusScore;

    public int appliedScoreValue;

    public bool isForceOdd = false;
    public bool isForceEven = false;
    public bool isScoreUnLocked = false;
    
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
        isScoreUnLocked = false;
        appliedScoreValue = 0;

        if(data != null)
        {
            multiBonusScore = data.multiBonusScore;
            plusBonusScore = data.plusBonusScore;

        }
        else
        {
            this.multiBonusScore = 1;
            this.plusBonusScore = 0;
        }
        this.isForceOdd = false;
        this.isForceEven = false;
    }

    public int ApplyDiceScoreChange(int diceScore)
    {
        if (isScoreUnLocked) return 0;
        int diff = diceScore - appliedScoreValue;
        scoreValue = diceScore;
        appliedScoreValue = diceScore;
        return diff;
    }
}
