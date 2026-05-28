using UnityEngine;

[CreateAssetMenu(fileName = "ShieldItem", menuName = "Scriptable Object/BattleItem/Shield")]
public class ShieldItemSo : BattleItemSo
{
    public int shield;

    public override void OnUse(BattleContext ctx)
    {
        ctx.Player.ShieldUp(shield);
        ctx.EventBus?.TriggerPlayerDefend(ctx);
    }
}
