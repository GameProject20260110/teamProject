using UnityEngine;

[System.Serializable]
public class BattleSaveData
{
    [Header("Player Data")]
    public int playerCurrentHP = 50;
    public int playerMaxHP = 50;

    [Header("Enemy Data")]
    public int enemyCurrentHP;
    public int enemyMaxHP;
    public string enemyName = "";

    [Header("Battle State")]
    public bool isPlayerTurn = true;
    public bool isBattleActive = false;
    public int currentBattleRound = 1; // 전투 내 라운드 (주사위 굴린 횟수)

    // 기본값 생성자
    public BattleSaveData() { }

    // 편의 생성자
    public BattleSaveData(PlayerBattleData player, EnemyBattleData enemy, bool playerTurn, bool active)
    {
        this.playerCurrentHP = player.CurrentHP;
        this.playerMaxHP = player.MaxHp;

        this.enemyCurrentHP = enemy.CurrentHP;
        this.enemyMaxHP = enemy.MaxHp;

        this.isPlayerTurn = playerTurn;
        this.isBattleActive = active;
    }
}