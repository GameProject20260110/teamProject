using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Shoes", menuName = "ArtifactItem/Shoes")]
public class Shoes : BattleItemSo
{
    [Range(0f, 1f)]
    public float damagePercent = 0.25f;

    public bool turnOn = true;
    private int bonusDamage = 0;

    public override void OnUse(DiceContext ctx) { }

    public override void OnEquip(BattleEventBus bus)
    {
        bus.OnPlayerAttackBefore += HandlePlayerAttackBefore;
        bus.OnPlayerAttackEnd += HandlePlayerAttackEnd;
    }

    public override void OnUnequip(BattleEventBus bus)
    {
        bus.OnPlayerAttackBefore -= HandlePlayerAttackBefore;
        bus.OnPlayerAttackEnd -= HandlePlayerAttackEnd;
    }

    private async UniTask HandlePlayerAttackBefore(BattleContext ctx)
    {
        bonusDamage = Mathf.RoundToInt(ctx.Enemy.CurrentShield * damagePercent);
        turnOn = bonusDamage > 0;

        if (!turnOn) return;

        ArtifactUIController.instance?.PlayTelegraph(this, true);
        await UniTask.Delay(200);
    }

    private async UniTask HandlePlayerAttackEnd(BattleContext ctx)
    {
        if (turnOn)
        {
            ArtifactUIController.instance?.PlayEffect(this);
            await PlayEffectAsync(ctx, bonusDamage);
            ArtifactUIController.instance?.PlayTelegraph(this, false);
        }

        turnOn = true;
    }

    private async UniTask PlayEffectAsync(BattleContext ctx, int damage)
    {
        await UniTask.Delay(200);

        BattleManager.instance?.ShowBonusDamageText(damage);
        AudioManager.instance?.PlaySfx("ShieldAttack");
        ctx.Enemy.TakeDamageRaw(damage);
    }
}
