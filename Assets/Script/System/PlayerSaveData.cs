using System.Collections.Generic;

[System.Serializable]
public class PlayerSaveData
{
    public int gold;
    public int currentRound;
    public int heart;
    public bool[] specialSlots;
    public List<string> diceNames = new List<string>();
    public List<string> itemNames = new List<string>();
    public string extraDiceName;
}
