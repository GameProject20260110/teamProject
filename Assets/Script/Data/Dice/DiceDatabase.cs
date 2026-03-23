using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Dice/DiceDatabase")]
public class DiceDatabase : ScriptableObject
{
    public List<DiceData> allDices;

#if UNITY_EDITOR
    [ContextMenu("모든 주사위 로드")]
    private void LoadAllDices()
    {
        allDices = Resources.LoadAll<DiceData>("DiceDatas").ToList();
        allDices.Sort((a, b) => a.diceNum.CompareTo(b.diceNum));
    }
#endif
}
