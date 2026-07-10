using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Ring", menuName = "ArtifactItem/Ring")]
public class Ring : BattleItemSo
{
    [Range(0f, 1f)]
    public float healPercent = 0.15f;

    public override void OnUse(DiceContext ctx)
    {
        
    }

    public override void OnEquip(BattleEventBus bus)
    {
        bus.OnEnemyDead += HandleEnemyDead;
    }

    public override void OnUnequip(BattleEventBus bus)
    {
        bus.OnEnemyDead -= HandleEnemyDead;
    }

    private void HandleEnemyDead(BattleContext ctx)
    {
        int healAmount = Mathf.RoundToInt(ctx.Player.MaxHp * healPercent);
        ctx.Player.Heal(healAmount);
        
        ArtifactUIController.instance.PlayEffect(this);
        PlayEffectAsync(healAmount, ctx).Forget();
    }

    private async UniTaskVoid PlayEffectAsync(int healAmount, BattleContext ctx)
    {
        await UniTask.Delay(200);

        BattleManager.Instance?.ShowHealText(healAmount);
        //AudioManager.instance?.PlaySfx("Heal");
        //ArtifactUIController.instance?.PlayHealParticle();
        EffectManager.Instance.PlayHealEffect(ctx.Positions.PlayerPosition);
    }
}
