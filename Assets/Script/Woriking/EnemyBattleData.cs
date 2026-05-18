using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemyBattleData : IDamageable
{
    private EnemyData _data;  // 원본 참조
    private int currentHP;
    private List<StatusEffect> statusEffects = new();  // 번, 독 등

    public int MaxHp => _data.maxHp;
    public int CurrentHP => currentHP;
    public IReadOnlyList<StatusEffect> StatusEffects => statusEffects;

    public void Initialize(EnemyData data)
    {
        _data = data;
        currentHP = data.maxHp;
        statusEffects.Clear();
    }

    public void Initialize(EnemyData data, int savedHP)
    {
        _data = data;
        currentHP = savedHP;
        statusEffects.Clear();
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);
    }

    public void TakeDamageRaw(int damage)
    {
        currentHP = Mathf.Max(0, currentHP - damage);
    }

    public void ApplyStatusEffect(StatusEffect effect)
    {
        var existing = statusEffects.FirstOrDefault(e => e.effectName == effect.effectName);
        if (existing != null)
        {
            existing.duration = Mathf.Max(existing.duration, effect.duration);
            existing.value = Mathf.Max(existing.value, effect.value);
        }
        else
        {
            statusEffects.Add(effect);
        }
    }

    // 추가 - 적 턴 시작마다 호출
    public async UniTask ProcessTurnStart(BattleContext ctx)
    {
        foreach (var effect in statusEffects)
            await effect.OnTurnStart(this, ctx);

        statusEffects.RemoveAll(e => e.Tick());
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }
}