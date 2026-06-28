using Cysharp.Threading.Tasks;
using UnityEngine;

public class FireDiceVFX : DiceVFXBase
{
    public override async UniTask PlayAttack(DiceContext ctx, int damage)
    {
        var data = ctx.DiceData.effectData as FireEffectData;
        var completion = new UniTaskCompletionSource<bool>();
        GameObject skill = ObjectPool.instance.Get(data.attackPrefab);

        Vector3 startPos = transform.position;
        Vector3 targetPos = ctx.IsPlayer ? ctx.Positions.EnemyPosition : ctx.Positions.PlayerPosition;

        if (ctx.IsPlayer)
            ctx.Enemy.ApplyStatusEffect(new BurnEffect(data.burnDamage, data.burnDuration));
        else
            ctx.Player.ApplyStatusEffect(new BurnEffect(data.burnDamage, data.burnDuration));

        
        skill.transform.position = startPos;

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
        var data = ctx.DiceData.effectData as FireEffectData;
        var completion = new UniTaskCompletionSource<bool>();

        Vector3 pos = ctx.IsPlayer ? ctx.Positions.PlayerPosition : ctx.Positions.EnemyPosition;

        GameObject skill = ObjectPool.instance.Get(data.shieldPrefab);
        skill.transform.position = pos;
        skill.GetComponent<Skill>().Init(new SkillContext
        {
            isPlayer = true,
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
