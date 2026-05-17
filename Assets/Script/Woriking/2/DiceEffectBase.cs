using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public abstract class DiceEffectBase : MonoBehaviour
{
    protected Dice dice;
    protected DiceData data => dice.MyState.diceData;

    protected virtual void Awake() => dice = GetComponent<Dice>();

    // 기본 구현 제공 → 필요한 것만 override
    public virtual UniTask OnAttack(BattleContext ctx) => UniTask.CompletedTask;
    public virtual UniTask OnDefense(BattleContext ctx) => UniTask.CompletedTask;
    public virtual UniTask OnRoll(CancellationToken ct) => UniTask.CompletedTask;
}
