using UnityEngine;

public class BattleSaveHandler
{
    private const string BATTLE_SAVE_FILE = "battleData.json";
    private readonly PlayerBattleData _playerData;
    private readonly EnemyBattleData _enemyData;
    private readonly BattleUI _battleUI;
    private readonly PlayerData _playerSO;
    private readonly SaveManager _saveManager;
    private readonly BattleDataManager _battleDataManager;

    public BattleSaveHandler(
        PlayerBattleData playerData,
        EnemyBattleData enemyData,
        BattleUI battleUI,
        PlayerData playerSO,
        SaveManager saveManager,
        BattleDataManager battleDataManager)
    {
        _playerData = playerData;
        _enemyData = enemyData;
        _battleUI = battleUI;
        _playerSO = playerSO;
        _saveManager = saveManager;
        _battleDataManager = battleDataManager;
    }

    public void Save(bool isPlayerTurn, bool isBattleActive, int currentTurn)
    {
        if (_saveManager == null) return;
        BattleSaveData data = new BattleSaveData(_playerData, _enemyData, isPlayerTurn, isBattleActive);
        data.currentBattleRound = currentTurn;
        _saveManager.Save(data, BATTLE_SAVE_FILE);
        Debug.Log("전투 데이터 저장");
    }

    public BattleSaveData Load()
    {
        if (_saveManager == null) return null;
        if (!_saveManager.HasSaveFile(BATTLE_SAVE_FILE))
        {
            Debug.Log("저장된 전투 데이터 없음");
            return null;
        }
        if (_battleDataManager == null || _battleDataManager.GetEnemyMaxHp() == 0)
        {
            Debug.LogWarning("BattleDataManager 데이터 없음");
            _saveManager.Delete(BATTLE_SAVE_FILE);
            return null;
        }
        return _saveManager.Load<BattleSaveData>(BATTLE_SAVE_FILE);
    }

    public void Delete()
    {
        if (_saveManager != null)
        {
            _saveManager.Delete(BATTLE_SAVE_FILE);
            Debug.Log("전투 데이터 삭제");
        }
    }

    public bool HasSaveFile() => _saveManager?.HasSaveFile(BATTLE_SAVE_FILE) ?? false;
}
