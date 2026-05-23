using Cysharp.Threading.Tasks;

public class NormalDiceEffect : DiceEffectBase
{
    public override async UniTask OnAttack(BattleContext ctx)
    {
        await vfx.PlayAttack(ctx, ctx.BaseDamage);
    }

    public override async UniTask OnDefense(BattleContext ctx)
    {
        int finalShield = ctx.BaseDamage + ctx.diceData.effectData.bonusShield;

        if (ctx.diceData.effectData.bonusShield > 0)
            await vfx.PlayBuff(ctx.diceData.effectData.bonusShield, ctx.CancellationToken);

        await vfx.PlayDefense(ctx, finalShield);
    }
}
