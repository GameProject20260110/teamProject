using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class BoostDiceVFX : DiceVFXBase
{
    public override async UniTask PlayAttack(DiceContext ctx, int damage)
    {
        await PlayBoost(ctx, ctx.dices.attackDices);
    }

    public override async UniTask PlayDefense(DiceContext ctx, int damage)
    {
        await PlayBoost(ctx, ctx.dices.defenseDices);
    }

    private async UniTask PlayBoost(DiceContext ctx, List<Dice> sameLane)
    {
        Dice highest = null;
        foreach (var d in sameLane)
        {
            if (d.VFX == this) continue;
            if (highest == null || d.MyState.modifiedValue > highest.MyState.modifiedValue)
                highest = d;
        }

        if (highest == null) return;

        // 1. 자기 자신 번쩍
        await PlayBuff(0, ctx.CancellationToken);

        // 2. 대상 주사위로 이펙트 날아감
        var completion = new UniTaskCompletionSource<bool>();
        GameObject skill = ObjectPool.instance.Get(ctx.DiceData.effectData.attackPrefab);
        skill.transform.position = highest.transform.position;
        skill.GetComponent<Skill>().Init(new SkillContext
        {
            isPlayer = ctx.IsPlayer,
            damage = 0,
            onHit = () => { },
            onEnd = () => completion.TrySetResult(true),
            startPos = transform.position,
            targetPos = highest.transform.position
        });

        await completion.Task.AttachExternalCancellation(ctx.CancellationToken);

        // 3. 대상 주사위 펄스
        await highest.VFX.PlayBuff(ctx.diceState.modifiedValue, ctx.CancellationToken);
    }
}
