using UnityEngine;

public abstract class CardData : ScriptableObject
{
    [Header("카드 공통 정보")]
    public int cardID;
    public string cardName;

    [TextArea]
    public string description;

    [Header("코스트 (마나 소모량)")]
    [Min(0)]
    public int cost;

    [Header("공격력)")]
    [SerializeField] private int diceSides = 6;
    public DiceSkin diceSkin;
    public int DiceSides => diceSides;

    public int RollPower() => Random.Range(1, diceSides + 1);

    public abstract void ApplyEffect(CardRuntime runtime);
}
