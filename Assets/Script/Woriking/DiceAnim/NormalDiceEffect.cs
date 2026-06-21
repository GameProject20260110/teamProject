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
        int finalShield = ctx.baseDamage + ctx.diceData.effectData.bonusShield;

        if (ctx.diceData.effectData.bonusShield > 0)
            await vfx.PlayBuff(ctx.diceData.effectData.bonusShield, ctx.CancellationToken);

        await vfx.PlayDefense(ctx, finalShield);
    }
}
