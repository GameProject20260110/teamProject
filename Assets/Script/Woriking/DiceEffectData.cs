using UnityEngine;

[CreateAssetMenu(fileName = "DiceEffectData", menuName = "Scriptable Objects/DiceEffectData")]
public class DiceEffectData : ScriptableObject
{
    public GameObject attackPrefab;
    public GameObject shieldPrefab;
}
