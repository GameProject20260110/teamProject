using Cysharp.Threading.Tasks;

public class BurnEffect : StatusEffect
{
    public BurnEffect(int damage, int duration)
    {
        effectName = "È­»ó";
        this.duration = duration;
        value = damage;
    }

    public override async UniTask OnTurnStart(IDamageable target, DiceContext ctx)
    {
        var completion = new UniTaskCompletionSource<bool>();
        AudioManager.Instance.PlaySfx("Burn");
        EffectManager.Instance.PlayBurnEffect(target, value, ctx, () => completion.TrySetResult(true));
        await completion.Task.AttachExternalCancellation(ctx.CancellationToken);
    }
}