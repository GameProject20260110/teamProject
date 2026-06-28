using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class BoostDiceEffect : DiceEffectBase
{
    public override async UniTask OnAttack(DiceContext ctx)
    {
        Boost(ctx, ctx.dices.attackDices);
        await vfx.PlayAttack(ctx, 0);
    }

    public override async UniTask OnDefense(DiceContext ctx)
    {
        Boost(ctx, ctx.dices.defenseDices);
        await vfx.PlayDefense(ctx, 0);
    }

    private void Boost(DiceContext ctx, List<Dice> sameLane)
    {
        Dice highest = null;
        foreach (var d in sameLane)
        {
            if (d.Effect is BoostDiceEffect) continue;
            if (highest == null || d.MyState.modifiedValue > highest.MyState.modifiedValue)
                highest = d;
        }

        if (highest != null)
            highest.MyState.modifiedValue += ctx.diceState.modifiedValue;       
    }
}
