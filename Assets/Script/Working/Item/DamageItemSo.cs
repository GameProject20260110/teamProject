using UnityEngine;

[CreateAssetMenu(fileName = "DamageItem", menuName = "Scriptable Object/BattleItem/Damage")]
public class DamageItemSo : BattleItemSo
{
    public int damage;

    public override void OnUse(DiceContext ctx)
    {
        ctx.Enemy.TakeDamage(damage);
    }
}
