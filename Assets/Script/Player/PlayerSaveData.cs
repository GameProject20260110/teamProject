using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerSaveData
{
    public int gold;
    public int level;
    public int currentRound;
    public int maxLives;
    public int currentLives;
    public List<string> diceNames = new List<string>();
    public List<string> itemNames = new List<string>();
}
