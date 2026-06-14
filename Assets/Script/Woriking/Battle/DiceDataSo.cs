using UnityEngine;
using static ScoreManager;

[CreateAssetMenu(menuName = "Dice/DiceData")]
public class DiceDataSo : ScriptableObject
{
    public string diceName;
    public DiceSkin skin;
    public DiceType diceType;   // Normal, Fire, Poison, Heal...
    public int attackPower;
    public string description;
    // 이펙트 프리팹 레퍼런스도 여기
    public GameObject effectPrefab;
    public AudioClip rollSound;
}
