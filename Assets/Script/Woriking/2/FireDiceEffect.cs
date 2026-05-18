using Cysharp.Threading.Tasks;
using UnityEngine;

public class FireDiceEffect : DiceEffectBase
{
    [SerializeField] private int burnDamage = 1;
    [SerializeField] private int burnDuration = 2;
    [SerializeField] private GameObject firePrefab;

    public override async UniTask OnAttack(BattleContext ctx)
    {
        var completion = new UniTaskCompletionSource<bool>();

        ctx.Enemy.ApplyStatusEffect(new BurnEffect(burnDamage, burnDuration));

        GameObject skill = ObjectPool.instance.Get(firePrefab);
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
