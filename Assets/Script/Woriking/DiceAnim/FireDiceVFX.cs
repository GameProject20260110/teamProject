using Cysharp.Threading.Tasks;
using UnityEngine;

public class FireDiceVFX : DiceVFXBase
{
    public override async UniTask PlayAttack(DiceContext ctx, int damage)
    {
        var data = ctx.DiceData.effectData as FireEffectData;
        var completion = new UniTaskCompletionSource<bool>();

        Vector3 startPos = transform.position;
        Vector3 targetPos = ctx.IsPlayer ? ctx.Positions.EnemyPosition : ctx.Positions.PlayerPosition;

        GameObject skill = WorldPoolManager.instance.Get(data.attackPrefab, startPos,Quaternion.identity);

        if (ctx.IsPlayer)
            ctx.Enemy.ApplyStatusEffect(new BurnEffect(data.burnDamage, data.burnDuration));
        else
            ctx.Player.ApplyStatusEffect(new BurnEffect(data.burnDamage, data.burnDuration));

        skill.GetComponent<Skill>().Init(new SkillContext
        {
            isPlayer = ctx.IsPlayer,
            damage = damage,
            onHit = () =>
            {
                if (ctx.IsPlayer)
                {
                    ctx.Enemy.TakeDamage(damage,ctx);
                }
                else
                {
                    ctx.Player.TakeDamage(damage,ctx);
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
        var data = ctx.DiceData.effectData as FireEffectData;
        var completion = new UniTaskCompletionSource<bool>();

        Vector3 pos = ctx.IsPlayer ? ctx.Positions.PlayerPosition : ctx.Positions.EnemyPosition;

        GameObject skill = WorldPoolManager.instance.Get(data.shieldPrefab,pos,Quaternion.identity);
        
        skill.GetComponent<Skill>().Init(new SkillContext
        {
            isPlayer = true,
            damage = damage,
            onHit = () =>
            {
                if (ctx.IsPlayer)
                {
                    ctx.Player.ShieldUp(damage,ctx);
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
