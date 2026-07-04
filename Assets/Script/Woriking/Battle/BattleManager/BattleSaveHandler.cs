using UnityEngine;

public class BattleSaveHandler
{
    private const string BATTLE_SAVE_FILE = "battleData.json";

    private readonly PlayerBattleData _playerData;
    private readonly EnemyBattleData _enemyData;
    private readonly BattleUI _battleUI;
    private readonly PlayerData _playerSO;

    public BattleSaveHandler(
        PlayerBattleData playerData,
        EnemyBattleData enemyData,
        BattleUI battleUI,
        PlayerData playerSO)
    {
        _playerData = playerData;
        _enemyData = enemyData;
        _battleUI = battleUI;
        _playerSO = playerSO;
    }

    public void Save(bool isPlayerTurn, bool isBattleActive, int currentTurn)
    {
        if (SaveManager.instance == null) return;

        BattleSaveData data = new BattleSaveData(
            _playerData,
            _enemyData,
            isPlayerTurn,
            isBattleActive
        );
        data.currentBattleRound = currentTurn;

        SaveManager.instance.Save(data, BATTLE_SAVE_FILE);
        Debug.Log("전투 데이터 저장");
    }

    public BattleSaveData Load()
    {
        if (SaveManager.instance == null) return null;
        if (!SaveManager.instance.HasSaveFile(BATTLE_SAVE_FILE))
        {
            Debug.Log("저장된 전투 데이터 없음");
            return null;
        }
        if (BattleDataManager.instance == null || BattleDataManager.instance.GetEnemyMaxHp() == 0)
        {
            Debug.LogWarning("BattleDataManager 데이터 없음");
            SaveManager.instance.Delete(BATTLE_SAVE_FILE);
            return null;
        }

        return SaveManager.instance.Load<BattleSaveData>(BATTLE_SAVE_FILE);
    }

    public void Delete()
    {
        if (SaveManager.instance != null)
        {
            SaveManager.instance.Delete(BATTLE_SAVE_FILE);
            Debug.Log("전투 데이터 삭제");
        }
    }

    public bool HasSaveFile() => SaveManager.instance?.HasSaveFile(BATTLE_SAVE_FILE) ?? false;
}
