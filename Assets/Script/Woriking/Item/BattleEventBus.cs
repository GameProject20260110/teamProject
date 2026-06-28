using System;

public class BattleEventBus
{
    public event Action<DiceContext, int> OnHitEnemy;
    public event Action<DiceContext, int> OnPlayerHit;

    public event Action<BattleContext> OnEnemyDead;
    public event Action<BattleContext> OnPlayerDead;

    public event Action<BattleContext> OnTurnStart;
    public event Action<BattleContext> OnTurnEnd;
    
    public event Action<DiceContext> OnPlayerDefend;
    public event Action<DiceContext> OnEnemyDefend;

    public event Action<DiceContext> OnPlayerAttackStart;
    public event Action<DiceContext> OnEnemyAttackStart;

    public event Action<DiceContext, int> OnPlayerHeal;
    public event Action<DiceContext, int> OnEnemyHeal;

    // 아이템 이벤트
    public event Action<BattleContext> OnBattleEnd;
    public event Action<BattleContext> OnPlayerAttackEnd;
    public event Action<BattleContext> OnPlayerAttackBefore;




    public void TriggerPlayerHit(DiceContext ctx, int damage) => OnPlayerHit?.Invoke(ctx, damage);
    public void TriggerHitEnemy(DiceContext ctx, int damage) => OnHitEnemy?.Invoke(ctx, damage);

    public void TriggerEnemyDead(BattleContext ctx) => OnEnemyDead?.Invoke(ctx);
    public void TriggerPlayerDead(BattleContext ctx) => OnPlayerDead?.Invoke(ctx);

    public void TriggerTurnStart(BattleContext ctx) => OnTurnStart?.Invoke(ctx);
    public void TriggerTurnEnd(BattleContext ctx) => OnTurnEnd?.Invoke(ctx);

    public void TriggerPlayerAttackStart(DiceContext ctx) => OnPlayerAttackStart?.Invoke(ctx);
    public void TriggerEnemyAttackStart(DiceContext ctx) => OnEnemyAttackStart?.Invoke(ctx);

    public void TriggerPlayerDefend(DiceContext ctx) => OnPlayerDefend?.Invoke(ctx);
    public void TriggerEnemyDefend(DiceContext ctx) => OnEnemyDefend?.Invoke(ctx);

    public void TriggerPlayerHeal(DiceContext ctx, int Amount) => OnPlayerHeal?.Invoke(ctx, Amount);
    public void TriggerEnemyHeal(DiceContext ctx, int Amount) => OnEnemyHeal?.Invoke(ctx, Amount);

    // 아이템
    public void TriggerBattleEnd(BattleContext ctx) => OnBattleEnd?.Invoke(ctx);
    public void TriggerPlayerAttackBefore(BattleContext ctx) => OnPlayerAttackBefore?.Invoke(ctx);
    public void TriggerOnPlayerAttackEnd(BattleContext ctx) => OnPlayerAttackEnd?.Invoke(ctx); 
}
