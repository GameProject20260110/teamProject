using Cysharp.Threading.Tasks;
using UnityEngine;

public class NormalDiceVFX : DiceVFXBase
{
    public override async UniTask PlayAttack(BattleContext ctx, int damage)
    {
        var completion = new UniTaskCompletionSource<bool>();
        var data = ctx.diceData.effectData;

        GameObject skill = ObjectPool.instance.Get(data.attackPrefab);
        skill.transform.position = ctx.EnemyPosition;
        skill.GetComponent<Skill>().Init(new SkillContext
        {
            isPlayer = true,
            damage = damage,
            onHit = () =>
            {
                ctx.Enemy.TakeDamage(damage);
                ctx.OnEnemyHit?.Invoke(damage);
            },
            onEnd = () => completion.TrySetResult(true),
            startPos = ctx.PlayerPosition,
            targetPos = ctx.EnemyPosition
        });

        await completion.Task.AttachExternalCancellation(ctx.CancellationToken);
    }

    public override async UniTask PlayDefense(BattleContext ctx, int damage)
    {
        var completion = new UniTaskCompletionSource<bool>();
        var data = ctx.diceData.effectData;

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