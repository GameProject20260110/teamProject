using UnityEngine;

[CreateAssetMenu(menuName = "Battle/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHp;
    public int attackPower;
    public Sprite enemyImage;
    public GameObject skillPrefab;
    // 패턴, 드롭 아이템 등 나중에 확장
}
