using System;
using System.Threading;
using UnityEngine;

public class BattleContext
{
    public PlayerBattleData Player;
    public EnemyBattleData Enemy;
    public Vector3 EnemyPosition;
    public Vector3 PlayerPosition;
    public int BaseDamage;
    public CancellationToken CancellationToken;
    public DiceData diceData;

    public Action<int> OnEnemyHit;
    public Action<int> OnPlayerDefend;
}