using Cysharp.Threading.Tasks;
using UnityEngine;

public class FireDiceEffect : DiceEffectBase
{
    [SerializeField] private int burnDamage = 1;
    [SerializeField] private int burnDuration = 2;
    [SerializeField] private GameObject firePrefab;

    public override async UniTask OnAttack(BattleContext ctx)
    {
        // 1. 기본 공격은 BattleContext가 처리
        // 2. 추가 효과만 여기서
        ctx.Enemy.ApplyStatusEffect(new BurnEffect(burnDamage, burnDuration));

        var vfx = ObjectPool.instance.Get(firePrefab);
        vfx.transform.position = ctx.EnemyPosition;

        await UniTask.Delay(300, cancellationToken: ctx.CancellationToken);
    }
}
