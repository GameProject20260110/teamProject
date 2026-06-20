using Cysharp.Threading.Tasks;
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
        //bus.OnPlayerAttackBefore += HandlePlayerAttackBefore; 시작 전에 발동 시 이거 씀
    }

    public override void OnUnequip(BattleEventBus bus)
    {
        bus.OnPlayerAttackEnd -= HandlePlayerAttackEnd;
        //bus.OnPlayerAttackBefore += HandlePlayerAttackBefore;
    }

    private void HandlePlayerAttackEnd(BattleContext ctx)
    {
        int bonusDamage = Mathf.RoundToInt(ctx.Enemy.CurrentShield * damagePercent);
        if (bonusDamage <= 0) return;

        ArtifactUIController.instance?.PlayEffect(this);
        PlayEffectAsync(ctx, bonusDamage).Forget();
    }

    //private void HandlePlayerAttackBefore(BattleContext ctx)
    //{
    //    int bonusDamage = Mathf.RoundToInt(ctx.Enemy.CurrentShield * damagePercent);
    //    if (bonusDamage <= 0) return;

    //    ArtifactUIController.instance?.PlayEffect(this);
    //    PlayEffectAsync(ctx, bonusDamage).Forget();
    //}

    private async UniTaskVoid PlayEffectAsync(BattleContext ctx, int damage)
    {
        await UniTask.Delay(200);

        BattleManager.instance?.ShowBonusDamageText(damage);
        AudioManager.instance?.PlaySfx(AudioManager.Sfx.ShieldAttack);
        ctx.Enemy.TakeDamageRaw(damage);
    }
}
