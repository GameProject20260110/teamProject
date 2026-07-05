using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BattleContextFactory
{
    private readonly PlayerBattleData _player;
    private readonly EnemyBattleData _enemy;
    private readonly Func<Transform> _getPlayerTransform;
    private readonly Func<Transform> _getEnemyTransform;
    private readonly BattleEventBus _eventBus;
    private readonly CancellationToken _cancellationToken;
    private readonly Func<int> _getCurrentTurn;

    public BattleContextFactory(
        PlayerBattleData player,
        EnemyBattleData enemy,
        Func<Transform> getPlayerTransform,
        Func<Transform> getEnemyTransform,
        BattleEventBus eventBus,
        CancellationToken cancellationToken,
        Func<int> getCurrentTurn)
    {
        _player = player;
        _enemy = enemy;
        _getPlayerTransform = getPlayerTransform;
        _getEnemyTransform = getEnemyTransform;
        _eventBus = eventBus;
        _cancellationToken = cancellationToken;
        _getCurrentTurn = getCurrentTurn;
    }

    public BattleContext CreateCtx(bool isPlayer = true)
    {
        return new BattleContext
        {
            Player = _player,
            Enemy = _enemy,
            IsPlayer = isPlayer,
            EventBus = _eventBus,
            CancellationToken = _cancellationToken,
            GetCurrentTurn = _getCurrentTurn,
            Positions = new BattlePositions
            {
                EnemyPosition = _getEnemyTransform().position,
                PlayerPosition = _getPlayerTransform().position
            }
        };
    }

    public DiceContext CreateDiceCtx(bool isPlayer, Dice dice, List<Dice> attack, List<Dice> defense)
    {
        if (dice == null)
        {
            return new DiceContext
            {
                battle = CreateCtx(isPlayer),
                baseDamage = 0,
                diceState = null,
                dices = new BattleDices { attackDices = attack, defenseDices = defense }
            };
        }

        return new DiceContext
        {
            battle = CreateCtx(isPlayer),
            baseDamage = dice.MyState.modifiedValue,
            diceState = dice.MyState,
            dices = new BattleDices { attackDices = attack, defenseDices = defense }
        };
    }
}