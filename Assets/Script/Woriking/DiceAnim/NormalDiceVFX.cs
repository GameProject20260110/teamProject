using Cysharp.Threading.Tasks;
using UnityEngine;

public class NormalDiceVFX : DiceVFXBase
{
    public override async UniTask PlayAttack(DiceContext ctx, int damage)
    {
        var completion = new UniTaskCompletionSource<bool>();
        var data = ctx.DiceData.effectData;
        GameObject skill = ObjectPool.instance.Get(data.attackPrefab);

        Vector3 startPos = ctx.IsPlayer ? ctx.Positions.PlayerPosition : ctx.Positions.EnemyPosition;
        Vector3 targetPos = ctx.IsPlayer ? ctx.Positions.EnemyPosition : ctx.Positions.PlayerPosition;

        skill.transform.position = targetPos;
        skill.GetComponent<Skill>().Init(new SkillContext
        {
            isPlayer = ctx.IsPlayer,
            damage = damage,
            onHit = () =>
            {
                if (ctx.IsPlayer)
                {
                    ctx.Enemy.TakeDamage(damage);
                    ctx.EventBus.TriggerHitEnemy(ctx, damage);
                }
                else
                {
                    ctx.Player.TakeDamage(damage);
                    ctx.EventBus.TriggerPlayerHit(ctx, damage);
                }
                
            },
            onEnd = () => completion.TrySetResult(true),
            startPos = startPos,
            targetPos = targetPos
        });

        await completion.Task.AttachExternalCancellation(ctx.CancellationToken);
    }

    public override async UniTask PlayDefense(DiceContext ctx, int damage)
    {
        var completion = new UniTaskCompletionSource<bool>();
        var data = ctx.DiceData.effectData;

        GameObject skill = ObjectPool.instance.Get(data.shieldPrefab);

        Vector3 pos = ctx.IsPlayer ? ctx.Positions.PlayerPosition : ctx.Positions.EnemyPosition;

        skill.transform.position = pos;
        skill.GetComponent<Skill>().Init(new SkillContext
        {
            isPlayer = ctx.IsPlayer,
            damage = damage,
            onHit = () =>
            {
                if (ctx.IsPlayer)
                {
                    ctx.Player.ShieldUp(damage);
                    ctx.EventBus.TriggerPlayerDefend(ctx);
                }
                else
                {
                    ctx.Enemy.ShieldUp(damage);
                    ctx.EventBus.TriggerEnemyDefend(ctx);
                }
                
            },
            onEnd = () => completion.TrySetResult(true),
            startPos = pos,
            targetPos = pos
        });

        await completion.Task.AttachExternalCancellation(ctx.CancellationToken);
    }
}