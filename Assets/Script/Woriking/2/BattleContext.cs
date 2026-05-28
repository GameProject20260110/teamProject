using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BattleContext
{
    public PlayerBattleData Player;
    public EnemyBattleData Enemy;
    public int BaseDamage;
    public DiceData diceData;
    public CancellationToken CancellationToken;
    public BattleEventBus EventBus;
    public bool IsPlayer;
    public BattlePositions Positions;

    public BattleDices Dices;
}

public class BattlePositions
{
    public Vector3 EnemyPosition;
    public Vector3 PlayerPosition;
}

public class BattleDices
{
    public List<Dice> AttackDices;
    public List<Dice> DefenseDices;
}