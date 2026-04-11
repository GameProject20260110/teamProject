using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DiceGacha/DiceGachaDataBase")]
public class DiceGachaDatabase : ScriptableObject
{
    public List<DiceGachaTable> diceGachaList;
}
