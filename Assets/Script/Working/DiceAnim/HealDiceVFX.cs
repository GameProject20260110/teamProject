using Cysharp.Threading.Tasks;
using UnityEngine;

public class HealDiceVFX : DiceVFXBase
{
    
    public override async UniTask PlayAttack(DiceContext ctx, int damage)
    {
        await PlayHeal(ctx, damage);
    }

    public override async UniTask PlayDefense(DiceContext ctx, int damage)
    {
        await PlayHeal(ctx, damage);
    }

    private async UniTask PlayHeal(DiceContext ctx, int damage)
    {
        var completion = new UniTaskCompletionSource<bool>();
        var data = ctx.DiceData.effectData;

        Vector3 startPos = transform.position;
        Vector3 targetPos = ctx.IsPlayer ? ctx.Positions.PlayerPosition : ctx.Positions.EnemyPosition;

        GameObject skill = WorldPoolManager.instance.Get(data.attackPrefab, targetPos, Quaternion.identity);
        
        skill.GetComponent<Skill>().Init(new SkillContext
        {
            isPlayer = ctx.IsPlayer,
            damage = damage,
            onHit = () =>
            {
                if (ctx.IsPlayer)
                {
                    ctx.Player.Heal(damage);
                    ctx.EventBus.TriggerPlayerHeal(ctx ,damage);
                }
                else
                {
                    ctx.Enemy.Heal(damage);
                    ctx.EventBus.TriggerEnemyHeal(ctx, damage);
                }

            },
            onEnd = () => completion.TrySetResult(true),
            startPos = startPos,
            targetPos = targetPos
        });

        await completion.Task.AttachExternalCancellation(ctx.CancellationToken);
    }

}
