using UnityEngine;

[CreateAssetMenu(fileName = "HealItem", menuName = "Scriptable Object/BattleItem/Heal")]
public class HealItemSo : BattleItemSo
{
    public int healAmount;

    public override void OnUse(DiceContext ctx) { }

    public override void OnEquip(BattleEventBus bus)
    {
        bus.OnBattleEnd += HandleBattleEnd;
    }

    public override void OnUnequip(BattleEventBus bus)
    {
        bus.OnBattleEnd -= HandleBattleEnd;
    }

    private void HandleBattleEnd(BattleContext ctx)
    {
        ctx.Player.Heal(healAmount);
    }
}
