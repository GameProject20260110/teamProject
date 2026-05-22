using Cysharp.Threading.Tasks;
using UnityEngine;

public class FireDiceEffect : DiceEffectBase
{
    public override async UniTask OnAttack(BattleContext ctx)
    {
        var data = ctx.diceData.effectData as FireEffectData;
        var completion = new UniTaskCompletionSource<bool>();
 
        ctx.Enemy.ApplyStatusEffect(new BurnEffect(data.burnDamage, data.burnDuration));

        GameObject skill = ObjectPool.instance.Get(data.attackPrefab);
        skill.transform.position = ctx.EnemyPosition;

        skill.GetComponent<Skill>().Init(new SkillContext {
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
}
