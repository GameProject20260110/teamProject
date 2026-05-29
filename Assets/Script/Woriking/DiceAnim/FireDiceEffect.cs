using Cysharp.Threading.Tasks;

public class FireDiceEffect : DiceEffectBase
{
    public override async UniTask OnAttack(DiceContext ctx)
    {
        int finalDamage = ctx.baseDamage + ctx.diceData.effectData.bonusDamage;

        if (ctx.diceData.effectData.bonusDamage > 0)
            await vfx.PlayBuff(ctx.diceData.effectData.bonusDamage, ctx.CancellationToken);

        await vfx.PlayAttack(ctx, finalDamage);
    }

    public override async UniTask OnDefense(DiceContext ctx)
    {
        await vfx.PlayDefense(ctx, ctx.baseDamage);
    }
}
