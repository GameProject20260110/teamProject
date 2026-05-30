using Cysharp.Threading.Tasks;

public abstract class StatusEffect
{
    public string effectName;
    public int duration;        // 남은 턴 수
    public int value;           // 데미지, 감소량 등

    // 턴 시작마다 호출
    public abstract UniTask OnTurnStart(IDamageable target, DiceContext ctx);

    // 한 턴 소모
    public bool Tick()
    {
        duration--;
        return duration <= 0; // true면 제거
    }
}