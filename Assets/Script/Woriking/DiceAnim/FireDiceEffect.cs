using Cysharp.Threading.Tasks;

public class FireDiceEffect : DiceEffectBase
{
    public override async UniTask OnAttack(BattleContext ctx)
    {
        int finalDamage = ctx.BaseDamage + ctx.diceData.effectData.bonusDamage;

        if (ctx.diceData.effectData.bonusDamage > 0)
            await vfx.PlayBuff(ctx.diceData.effectData.bonusDamage, ctx.CancellationToken);

        await vfx.PlayAttack(ctx, finalDamage);
    }

    public override async UniTask OnDefense(BattleContext ctx)
    {
        await vfx.PlayDefense(ctx, ctx.BaseDamage);
    }
}
