using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class BoostDiceEffect : DiceEffectBase
{
    public override async UniTask OnAttack(DiceContext ctx)
    {
        await Boost(ctx, ctx.dices.attackDices);
    }

    public override async UniTask OnDefense(DiceContext ctx)
    {
        await Boost(ctx, ctx.dices.defenseDices);
    }

    private async UniTask Boost(DiceContext ctx, List<Dice> sameLane)
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

        await vfx.PlayAttack(ctx, 0);
    }
}
