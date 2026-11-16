public class BattleUIBinder
{
    private readonly BattleUI _battleUI;
    private readonly PlayerBattleData _playerData;
    private readonly EnemyBattleData _enemyData;

    public BattleUIBinder(BattleUI battleUI, PlayerBattleData playerData, EnemyBattleData enemyData)
    {
        _battleUI = battleUI;
        _playerData = playerData;
        _enemyData = enemyData;
    }

    public void Subscribe(BattleEventBus eventBus)
    {
        eventBus.OnPlayerHeal += HandlePlayerHeal;
        eventBus.OnEnemyHeal += HandleEnemyHeal;
        eventBus.OnHitEnemy += HandleHitEnemy;
        eventBus.OnPlayerDefend += HandlePlayerDefend;
        eventBus.OnPlayerHit += HandleHitPlayer;
        eventBus.OnEnemyDefend += HandleEnemyDefend;
    }

    public void Unsubscribe(BattleEventBus eventBus)
    {
        eventBus.OnPlayerHeal -= HandlePlayerHeal;
        eventBus.OnEnemyHeal -= HandleEnemyHeal;
        eventBus.OnHitEnemy -= HandleHitEnemy;
        eventBus.OnPlayerDefend -= HandlePlayerDefend;
        eventBus.OnPlayerHit -= HandleHitPlayer;
        eventBus.OnEnemyDefend -= HandleEnemyDefend;
    }

    private void HandlePlayerHeal(DiceContext ctx, int amount)
        => _battleUI.UpdatePlayerHP(_playerData.CurrentHP, _playerData.MaxHp);

    private void HandleEnemyHeal(DiceContext ctx, int amount)
        => _battleUI.UpdateEnemyHP(_enemyData.CurrentHP, _enemyData.MaxHp);

    private void HandleHitPlayer(DiceContext ctx, int damage)
    {
        _battleUI.UpdatePlayerShield(_playerData.CurrentShield);
        _battleUI.UpdatePlayerHP(_playerData.CurrentHP, _playerData.MaxHp);
        _battleUI.ShowDamageText(damage, isPlayer: true);
    }

    private void HandleHitEnemy(DiceContext ctx, int damage)
    {
        _battleUI.UpdateEnemyShield(_enemyData.CurrentShield);
        _battleUI.UpdateEnemyHP(_enemyData.CurrentHP, _enemyData.MaxHp);
        _battleUI.ShowDamageText(damage, isPlayer: false);
    }

    private void HandleEnemyDefend(DiceContext ctx)
        => _battleUI.UpdateEnemyShield(_enemyData.CurrentShield);

    private void HandlePlayerDefend(DiceContext ctx)
        => _battleUI.UpdatePlayerShield(_playerData.CurrentShield);
}
