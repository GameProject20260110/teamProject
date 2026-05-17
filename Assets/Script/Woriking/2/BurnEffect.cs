using UnityEngine;

public class BurnEffect : StatusEffect
{
    public BurnEffect(int damage, int duration)
        : base("화상", duration, damage) { }

    public override void OnTurnStart(IDamageable target)
    {
        target.TakeDamageRaw(value); // 쉴드 무시 데미지
    }
}