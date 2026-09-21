using System;
using System.Threading;
using UnityEngine;

public class BattleContext
{
    public PlayerBattleData Player;
    public EnemyBattleData Enemy;
    public CancellationToken CancellationToken;
    public BattleEventBus EventBus;
    public bool IsPlayer;
    public BattlePositions Positions;
    public Func<int> GetCurrentTurn;

    public int CurrentTurn => GetCurrentTurn?.Invoke() ?? 1;
}

public class BattlePositions
{
    public Vector3 EnemyPosition;
    public Vector3 PlayerPosition;
}