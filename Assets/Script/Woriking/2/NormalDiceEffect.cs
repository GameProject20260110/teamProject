using Cysharp.Threading.Tasks;
using UnityEngine;

public class NormalDiceEffect : DiceEffectBase
{
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private GameObject shieldPrefab;

    public override async UniTask OnAttack(BattleContext ctx)
    {
        var completion = new UniTaskCompletionSource<bool>();

        GameObject skill = ObjectPool.instance.Get(attackPrefab);
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
            startPos = ctx.PlayerPosition,
            targetPos = ctx.EnemyPosition
        });

        await completion.Task.AttachExternalCancellation(ctx.CancellationToken);
    }

    public override async UniTask OnDefense(BattleContext ctx)
    {
        var completion = new UniTaskCompletionSource<bool>();

        GameObject skill = ObjectPool.instance.Get(shieldPrefab);
        skill.transform.position = ctx.PlayerPosition;

        skill.GetComponent<Skill>().Init(new SkillContext {
            isPlayer = true,
            damage = ctx.BaseDamage,
            onHit = () =>
            {
                ctx.Player.ShieldUp(ctx.BaseDamage);
                ctx.OnPlayerDefend?.Invoke(ctx.BaseDamage);
            },
            onEnd = () => completion.TrySetResult(true),
            startPos = ctx.PlayerPosition,
            targetPos = ctx.EnemyPosition
        });

        await completion.Task.AttachExternalCancellation(ctx.CancellationToken);
    }
}
