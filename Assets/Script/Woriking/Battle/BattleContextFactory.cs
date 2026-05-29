using System.Threading;
using UnityEngine;

public class BattleContextFactory
{
    private readonly PlayerBattleData _playerData;
    private readonly EnemyBattleData _enemyData;
    private readonly Transform _enemyTrans;
    private readonly Transform _playerTrans;
    private readonly BattleEventBus _eventBus;
    private readonly CancellationToken _ct;
    private readonly bool _isPlayer;

    public BattleContextFactory(
        PlayerBattleData playerData,
        EnemyBattleData enemyData,
        Transform enemyTrans,
        Transform playerTrans,
        BattleEventBus eventBus,
        CancellationToken ct,
        bool isPlayer = true)
    {
        _playerData = playerData;
        _enemyData = enemyData;
        _enemyTrans = enemyTrans;
        _playerTrans = playerTrans;
        _eventBus = eventBus;
        _ct = ct;
        _isPlayer = isPlayer;
    }

    public BattleContext Create()
    {
        return new BattleContext
        {
            Player = _playerData,
            Enemy = _enemyData,
            IsPlayer = _isPlayer,
            CancellationToken = _ct,
            EventBus = _eventBus,
            Positions = new BattlePositions
            {
                EnemyPosition = _enemyTrans.position,
                PlayerPosition = _playerTrans.position
            },
        };
    }
}