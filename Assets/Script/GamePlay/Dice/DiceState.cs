public class DiceState
{
    public DiceData diceData;
    public int diceIndex;      // 주사위 순서
    public int originalValue;  // 최초 주사위 값
    public int modifiedValue;  // 효과 적용 후 주사위 값
    public bool isFrozen;

    public DiceState(DiceData data, int index, int value)
    {
        diceData = data;
        diceIndex = index;
        originalValue = value;
        modifiedValue = value;
        isFrozen = false;
    }
}