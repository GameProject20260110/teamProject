using Cysharp.Threading.Tasks;
using UnityEngine;

public class BurnEffect : StatusEffect
{
    public BurnEffect(int damage, int duration)
    {
        effectName = "È­»ó";
        this.duration = duration;
        value = damage;
    }

    public override async UniTask OnTurnStart(IDamageable target, BattleContext ctx)
    {
        var completion = new UniTaskCompletionSource<bool>();
        EffectManager.instance.PlayBurnEffect(target, value, ctx, () => completion.TrySetResult(true));
        await completion.Task.AttachExternalCancellation(ctx.CancellationToken);
    }
}