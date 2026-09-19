using Cysharp.Threading.Tasks;
using UnityEngine;

public class NormalDiceVFX : DiceVFXBase
{
    public override async UniTask PlayAttack(DiceContext ctx, int damage)
    {
        var completion = new UniTaskCompletionSource<bool>();
        var data = ctx.DiceData.effectData;
        
        Vector3 startPos = ctx.IsPlayer ? ctx.Positions.PlayerPosition : ctx.Positions.EnemyPosition;
        Vector3 targetPos = ctx.IsPlayer ? ctx.Positions.EnemyPosition : ctx.Positions.PlayerPosition;
        GameObject skill = WorldPoolManager.instance.Get(data.attackPrefab, targetPos, Quaternion.identity);

        skill.GetComponent<Skill>().Init(new SkillContext
        {
            isPlayer = ctx.IsPlayer,
            damage = damage,
            onHit = () =>
            {
                if (ctx.IsPlayer)
                {
                    ctx.Enemy.TakeDamage(damage, ctx);
                }
                else
                {
                    ctx.Player.TakeDamage(damage, ctx);
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

        Vector3 pos = ctx.IsPlayer ? ctx.Positions.PlayerPosition : ctx.Positions.EnemyPosition;
        
        GameObject skill = WorldPoolManager.instance.Get(data.shieldPrefab, pos, Quaternion.identity);

        skill.GetComponent<Skill>().Init(new SkillContext
        {
            isPlayer = ctx.IsPlayer,
            damage = damage,
            onHit = () =>
            {
                if (ctx.IsPlayer)
                {
                    ctx.Player.ShieldUp(damage, ctx);
                }
                else
                {
                    ctx.Enemy.ShieldUp(damage, ctx);
                }
                
            },
            onEnd = () => completion.TrySetResult(true),
            startPos = pos,
            targetPos = pos
        });

        await completion.Task.AttachExternalCancellation(ctx.CancellationToken);
    }
}