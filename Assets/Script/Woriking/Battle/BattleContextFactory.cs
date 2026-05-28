using System.Collections.Generic;
using UnityEngine;

public class BattleContextFactory
{
    private readonly PlayerBattleData _playerData;
    private readonly EnemyBattleData _enemyData;
    private readonly Transform _enemyTrans;
    private readonly Transform _playerTrans;
    private readonly List<Dice> _attackDices;
    private readonly List<Dice> _defenseDices;
    private readonly BattleEventBus _eventBus;
    private readonly System.Threading.CancellationToken _ct;
    private readonly bool _isPlayer;

    public BattleContextFactory(
        PlayerBattleData playerData,
        EnemyBattleData enemyData,
        Transform enemyTrans,
        Transform playerTrans,
        List<Dice> attackDices,
        List<Dice> defenseDices,
        BattleEventBus eventBus,
        System.Threading.CancellationToken ct,
        bool isPlayer = true)
    {
        _playerData = playerData;
        _enemyData = enemyData;
        _enemyTrans = enemyTrans;
        _playerTrans = playerTrans;
        _attackDices = attackDices;
        _defenseDices = defenseDices;
        _eventBus = eventBus;
        _ct = ct;
        _isPlayer = isPlayer;
    }

    public BattleContext Create(int baseDamage = 0, DiceData diceData = null)
    {
        return new BattleContext
        {
            Player = _playerData,
            Enemy = _enemyData,
            BaseDamage = baseDamage,
            diceData = diceData,
            IsPlayer = _isPlayer,
            CancellationToken = _ct,
            EventBus = _eventBus,
            Positions = new BattlePositions
            {
                EnemyPosition = _enemyTrans.position,
                PlayerPosition = _playerTrans.position
            },
            Dices = new BattleDices
            {
                AttackDices = _attackDices,
                DefenseDices = _defenseDices
            }
        };
    }
}