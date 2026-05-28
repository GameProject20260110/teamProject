using UnityEngine;

[CreateAssetMenu(fileName = "DiceEffectData", menuName = "Scriptable Objects/DiceEffectData")]
public class DiceEffectData : ScriptableObject
{
    public GameObject attackPrefab;
    public GameObject shieldPrefab;

    [Header("bonus stats")]
    public int bonusDamage = 0;
    public int bonusShield = 0;
}
