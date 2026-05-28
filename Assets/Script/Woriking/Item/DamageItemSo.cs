using UnityEngine;

[CreateAssetMenu(fileName = "DamageItem", menuName = "Scriptable Object/BattleItem/Damage")]
public class DamageItemSo : BattleItemSo
{
    public int damage;

    public override void OnUse(BattleContext ctx)
    {
        ctx.Enemy.TakeDamage(damage);
        ctx.EventBus.TriggerHitEnemy(ctx, damage);
    }
}
