using UnityEngine;

[CreateAssetMenu(fileName = "Shoes", menuName = "ArtifactItem/Shoes")]
public class Shoes : BattleItemSo
{
    [Range(0f, 1f)]
    public float damagePercent = 0.25f;

    public override void OnUse(DiceContext ctx) { }

    public override void OnEquip(BattleEventBus bus)
    {
        bus.OnPlayerAttackEnd += HandlePlayerAttackEnd;
    }

    public override void OnUnequip(BattleEventBus bus)
    {
        bus.OnPlayerAttackEnd -= HandlePlayerAttackEnd;
    }

    private void HandlePlayerAttackEnd(BattleContext ctx)
    {
        int bonusDamage = Mathf.RoundToInt(ctx.Enemy.CurrentShield * damagePercent);
        if (bonusDamage <= 0) return;

        ctx.Enemy.TakeDamageRaw(bonusDamage);
    }
}
