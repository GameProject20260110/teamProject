using UnityEngine;

public abstract class BaseEnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHp;
    public int attackPower;
    public Sprite enemyImage;
    public GameObject skillPrefab;
    public EnemyAIStrategy aiStrategy;
    // 패턴, 드롭 아이템 등 나중에 확장
    [Header("보상")]
    public RewardDataSo rewardData;
}
