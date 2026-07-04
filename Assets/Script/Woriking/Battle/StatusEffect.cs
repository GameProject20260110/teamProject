using Cysharp.Threading.Tasks;

public abstract class StatusEffect
{
    public string effectName;
    public int duration;
    public int value;

    public abstract UniTask OnTurnStart(IDamageable target, DiceContext ctx);

    public bool Tick()
    {
        duration--;
        return duration <= 0;
    }
}