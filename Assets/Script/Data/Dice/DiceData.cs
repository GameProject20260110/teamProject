using UnityEngine;

[CreateAssetMenu(fileName = "DiceData", menuName = "Scriptable Objects/DiceData")]
public class DiceData : ScriptableObject
{
    public enum DiceRole { Attack, Defense, Neutral }

    [Header("주사위 기본 정보")]
    public int diceNum;
    public string abilityName;

    [Header("주사위 종류")]
    public DiceRole aiRole = DiceRole.Neutral;

    [Range(1, 5)]
    public int tier = 1;

    public int gold;
    public int sell;
    
    [TextArea]
    public string Desc;

    [TextArea]
    public string effectDesc;

    [Header("주사위 스킨")]
    public DiceSkin skin;

    [Header("주사위 프래팹")]
    public GameObject dicePrefab;

    [Header("주사위 효과")]
    public DiceEffectData effectData;
}