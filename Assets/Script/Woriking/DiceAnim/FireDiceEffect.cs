using Cysharp.Threading.Tasks;

public class FireDiceEffect : DiceEffectBase
{
    public override async UniTask OnAttack(DiceContext ctx)
    {
        int damage = ctx.baseDamage + ctx.DiceData.effectData.bonusDamage;
        int finalDamage = GetFinalAttackDamage(ctx, damage);
        if (ctx.DiceData.effectData.bonusDamage > 0)
            await vfx.PlayBuff(ctx.DiceData.effectData.bonusDamage, ctx.CancellationToken);

        await vfx.PlayAttack(ctx, finalDamage);
    }

    public override async UniTask OnDefense(DiceContext ctx)
    {
        await vfx.PlayDefense(ctx, ctx.baseDamage);
    }
}
