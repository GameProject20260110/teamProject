using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Gimmick/Fear")]
public class FearGim : GimmickSo
{
    [Range(0f, 1f)] public float attackMultiplier = 0.7f; // 30% 감소

    public override void Register(BattleEventBus bus)
    {
        bus.OnTurnStart += OnTurnStart;
        bus.OnTurnEnd += OnTurnEnd;
    }

    public override void Unregister(BattleEventBus bus)
    {
        bus.OnTurnStart -= OnTurnStart;
        bus.OnTurnEnd -= OnTurnEnd;
    }

    private void OnTurnStart(BattleContext ctx)
    {
        if (!ShouldActivate(ctx.CurrentTurn)) return;

        // 이번 턴 플레이어 공격력 감소
        ctx.Player.SetAttackMultiplier(attackMultiplier);
        BossGimmickUIContainer.instance?.ActivateAsync(this).Forget();
    }

    private void OnTurnEnd(BattleContext ctx)
    {
        // 턴 끝나면 원복
        ctx.Player.ResetAttackMultiplier();
        BossGimmickUIContainer.instance?.Deactivate(this);
    }

    public override string GetActiveDesc()
        => $"공격력 {Mathf.RoundToInt((1f - attackMultiplier) * 100)}% 감소";
}
