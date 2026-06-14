using UnityEngine;

[CreateAssetMenu(menuName = "Battle/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("기본 스탯")]
    public string playerName;
    public int maxHp = 50;
    public int baseAttack = 10;

    [Header("비주얼")]
    public Sprite portrait;
}
