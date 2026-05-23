using Cysharp.Threading.Tasks;
using UnityEngine;

public class FireDiceVFX : DiceVFXBase
{
    public override async UniTask PlayAttack(BattleContext ctx, int damage)
    {
        var data = ctx.diceData.effectData as FireEffectData;
        var completion = new UniTaskCompletionSource<bool>();

        ctx.Enemy.ApplyStatusEffect(new BurnEffect(data.burnDamage, data.burnDuration));

        GameObject skill = ObjectPool.instance.Get(data.attackPrefab);
        skill.transform.position = ctx.EnemyPosition;

        skill.GetComponent<Skill>().Init(new SkillContext
        {
            isPlayer = true,
            damage = ctx.BaseDamage,
            onHit = () =>
            {
                ctx.Enemy.TakeDamage(ctx.BaseDamage);
                ctx.OnEnemyHit?.Invoke(ctx.BaseDamage);
            },
            onEnd = () => completion.TrySetResult(true),
            startPos = transform.position,
            targetPos = ctx.EnemyPosition
        });

        await completion.Task.AttachExternalCancellation(ctx.CancellationToken);
    }

    public override async UniTask PlayDefense(BattleContext ctx, int damage)
    {
        var data = ctx.diceData.effectData as FireEffectData;
        var completion = new UniTaskCompletionSource<bool>();
        
        GameObject skill = ObjectPool.instance.Get(data.shieldPrefab);
        skill.transform.position = ctx.PlayerPosition;
        skill.GetComponent<Skill>().Init(new SkillContext
        {
            isPlayer = true,
            damage = damage,
            onHit = () =>
            {
                ctx.Player.ShieldUp(damage);
                ctx.OnPlayerDefend?.Invoke(damage);
            },
            onEnd = () => completion.TrySetResult(true),
            startPos = ctx.PlayerPosition,
            targetPos = ctx.EnemyPosition
        });

        await completion.Task.AttachExternalCancellation(ctx.CancellationToken);
    }
}
