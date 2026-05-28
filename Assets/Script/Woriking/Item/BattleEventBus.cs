using System;

public class BattleEventBus
{
    public event Action<BattleContext, int> OnHitEnemy;
    public event Action<BattleContext> OnEnemyDead;
    public event Action<BattleContext> OnTurnStart;
    public event Action<BattleContext> OnTurnEnd;
    public event Action<BattleContext, int> OnPlayerHit;
    public event Action<BattleContext> OnPlayerDefend;
    public event Action<BattleContext> OnEnemyDefend;

    public void TriggerHitEnemy(BattleContext ctx, int damage) => OnHitEnemy?.Invoke(ctx, damage);
    public void TriggerEnemyDead(BattleContext ctx) => OnEnemyDead?.Invoke(ctx);
    public void TriggerTurnStart(BattleContext ctx) => OnTurnStart?.Invoke(ctx);
    public void TriggerTurnEnd(BattleContext ctx) => OnTurnEnd?.Invoke(ctx);
    public void TriggerPlayerHit(BattleContext ctx, int damage) => OnPlayerHit?.Invoke(ctx, damage);
    public void TriggerPlayerDefend(BattleContext ctx) => OnPlayerDefend?.Invoke(ctx);
    public void TriggerEnemyDefend(BattleContext ctx) => OnEnemyDefend?.Invoke(ctx);
}
