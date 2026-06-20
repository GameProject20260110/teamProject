using System;

public class BattleEventBus
{
    public event Action<DiceContext, int> OnHitEnemy;
    public event Action<BattleContext> OnEnemyDead;
    public event Action<BattleContext> OnTurnStart;
    public event Action<BattleContext> OnTurnEnd;
    public event Action<DiceContext, int> OnPlayerHit;
    public event Action<DiceContext> OnPlayerDefend;
    public event Action<DiceContext> OnEnemyDefend;
    public event Action<BattleContext> OnBattleEnd;
    public event Action<BattleContext> OnPlayerAttackEnd;
    public event Action<BattleContext> OnPlayerAttackBefore;

    public void TriggerHitEnemy(DiceContext ctx, int damage) => OnHitEnemy?.Invoke(ctx, damage);
    public void TriggerEnemyDead(BattleContext ctx) => OnEnemyDead?.Invoke(ctx);
    public void TriggerTurnStart(BattleContext ctx) => OnTurnStart?.Invoke(ctx);
    public void TriggerTurnEnd(BattleContext ctx) => OnTurnEnd?.Invoke(ctx);
    public void TriggerPlayerAttackBefore(BattleContext ctx) => OnPlayerAttackBefore?.Invoke(ctx);
    public void TriggerPlayerHit(DiceContext ctx, int damage) => OnPlayerHit?.Invoke(ctx, damage);
    public void TriggerOnPlayerAttackEnd(BattleContext ctx) => OnPlayerAttackEnd?.Invoke(ctx);
    public void TriggerPlayerDefend(DiceContext ctx) => OnPlayerDefend?.Invoke(ctx);
    public void TriggerEnemyDefend(DiceContext ctx) => OnEnemyDefend?.Invoke(ctx);
    public void TriggerBattleEnd(BattleContext ctx) => OnBattleEnd?.Invoke(ctx);
    
}
