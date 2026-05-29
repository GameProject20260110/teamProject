using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlayerBattleData : IDamageable
{
    private PlayerData _data;
    private int currentHP;
    private int currentShield;
    private List<StatusEffect> statusEffects = new();

    public int MaxHp => _data.maxHp;
    public int CurrentHP => currentHP;
    public int CurrentShield => currentShield;
    public IReadOnlyList<StatusEffect> StatusEffects => statusEffects;

    public void Initialize(PlayerData data)
    {
        _data = data;
        currentHP = data.maxHp;
        currentShield = 0;
        statusEffects.Clear();
    }

    public void Initialize(PlayerData data, int savedHP)
    {
        _data = data;
        currentHP = savedHP;
        currentShield = 0;
        statusEffects.Clear();
    }

    public int TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(0, damage - currentShield);
        currentShield = Mathf.Max(0, currentShield - damage);
        currentHP = Mathf.Max(0, currentHP - actualDamage);
        return actualDamage;
    }

    public void TakeDamageRaw(int damage)
    {
        currentHP = Mathf.Max(0, currentHP - damage);
    }

    public void ShieldUp(int amount) => currentShield += amount;
    public void ResetShield() => currentShield = 0;

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

    public async UniTask ProcessTurnStart(DiceContext ctx)
    {
        foreach (var effect in statusEffects)
            await effect.OnTurnStart(this, ctx);

        statusEffects.RemoveAll(e => e.Tick());
    }

    public bool IsDead() => currentHP <= 0;
}
