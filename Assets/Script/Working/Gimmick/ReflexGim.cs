using UnityEngine;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(menuName = "Gimmick/Reflex")]
public class ReflexGim : GimmickSo
{
    public int reflectAmount;

    public override void Register(BattleEventBus eventBus)
    {
        eventBus.OnTurnStart += OnTurnStart;
        eventBus.OnTurnEnd += OnTurnEnd;
        eventBus.OnHitEnemy += OnHitEnemy;
    }

    public override void Unregister(BattleEventBus eventBus)
    {
        eventBus.OnTurnStart -= OnTurnStart;
        eventBus.OnTurnEnd -= OnTurnEnd;
        eventBus.OnHitEnemy -= OnHitEnemy;
    }

    private void OnTurnStart(BattleContext ctx)
    {
        if (!ShouldActivate(ctx.CurrentTurn)) return;
        BossGimmickUIContainer.instance.ActivateAsync(this).Forget();
    }

    private void OnTurnEnd(BattleContext ctx)
    {
        BossGimmickUIContainer.instance.Deactivate(this);
    }

    private void OnHitEnemy(DiceContext ctx, int damage)
    {
        if (!ShouldActivate(ctx.battle.CurrentTurn)) return;
        ctx.Player.TakeDamage(reflectAmount);
        ctx.EventBus.TriggerHitPlayer(ctx, reflectAmount);
    }

    public override string GetActiveDesc() => $"반사 {reflectAmount} 데미지";
}