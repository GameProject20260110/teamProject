using Cysharp.Threading.Tasks;

public class HealDiceEffect : DiceEffectBase
{
    public override async UniTask OnAttack(DiceContext ctx)
    {
        await Heal(ctx);
    }

    public override async UniTask OnDefense(DiceContext ctx)
    {
        await Heal(ctx);
    }

    private async UniTask Heal(DiceContext ctx)
    {
        int healAmount = ctx.baseDamage;
        await vfx.PlayAttack(ctx, healAmount);
    }
    
}
