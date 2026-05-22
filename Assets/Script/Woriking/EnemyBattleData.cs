using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemyBattleData : IDamageable
{
    private BaseEnemyData _data;  // 원본 참조
    private int currentHP;
    private List<StatusEffect> statusEffects = new();  // 번, 독 등

    public int MaxHp => _data.maxHp;
    public int CurrentHP => currentHP;
    public IReadOnlyList<StatusEffect> StatusEffects => statusEffects;

    public void Initialize(BaseEnemyData data)
    {
        _data = data;
        currentHP = data.maxHp;
        statusEffects.Clear();
    }

    public void Initialize(BaseEnemyData data, int savedHP)
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
    public void ProcessTurnStart()
    {
        foreach (var effect in statusEffects)
            effect.OnTurnStart(this);  // ← 근데 여기 문제 있음

        statusEffects.RemoveAll(e => e.Tick());
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }
}