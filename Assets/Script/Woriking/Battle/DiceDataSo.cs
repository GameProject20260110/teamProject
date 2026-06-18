using UnityEngine;

[CreateAssetMenu(menuName = "Dice/DiceData")]
public class DiceDataSo : ScriptableObject
{
    public string diceName;
    public DiceSkin skin;
    public int attackPower;
    public string description;
    // 이펙트 프리팹 레퍼런스도 여기
    public GameObject effectPrefab;
    public AudioClip rollSound;
}
