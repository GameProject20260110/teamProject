using UnityEngine;

public abstract class StatusEffect
{
    public string effectName;
    public int duration;        // 남은 턴 수
    public int value;           // 데미지, 감소량 등

    protected StatusEffect(string name, int duration, int value)
    {
        effectName = name;
        this.duration = duration;
        this.value = value;
    }

    // 턴 시작마다 호출
    public abstract void OnTurnStart(IDamageable target);

    // 한 턴 소모
    public bool Tick()
    {
        duration--;
        return duration <= 0; // true면 제거
    }
}