using Cysharp.Threading.Tasks;

public class NormalDiceEffect : DiceEffectBase
{
    public override async UniTask OnAttack(DiceContext ctx)
    {
        int damage = GetFinalAttackDamage(ctx, ctx.baseDamage);
        await vfx.PlayAttack(ctx, damage);
    }

    public override async UniTask OnDefense(DiceContext ctx)
    {
        int finalShield = ctx.baseDamage + ctx.DiceData.effectData.bonusShield;

        if (ctx.DiceData.effectData.bonusShield > 0)
            await vfx.PlayBuff(ctx.DiceData.effectData.bonusShield, ctx.CancellationToken);

        await vfx.PlayDefense(ctx, finalShield);
    }
}
