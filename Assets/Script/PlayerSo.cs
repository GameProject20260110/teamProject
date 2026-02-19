using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "playSo",menuName = "Scriptable Object/playerData")]
public class PlayerSo : ScriptableObject
{
    [Header("인 게임")]
    public List<ItemSo> itemSo = new List<ItemSo>();
    public DiceData[] DiceSo;
    public int gold;

    [Header("대기 화면")]
    public int bestRound;
    public int bestScore;
    
}
