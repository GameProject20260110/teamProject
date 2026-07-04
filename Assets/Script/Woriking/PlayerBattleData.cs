//using Cysharp.Threading.Tasks;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//[System.Serializable]
//public class PlayerBattleData : IDamageable
//{
//    private PlayerData _data;
//    private int currentHP;
//    private int currentShield;
//    private List<StatusEffect> statusEffects = new();

//    private float attackMultiplier = 1f;
//    public float AttackMultiplier => attackMultiplier;
//    public void SetAttackMultiplier(float value) => attackMultiplier = value;
//    public void ResetAttackMultiplier() => attackMultiplier = 1f;

//    public int MaxHp => _data.maxHp;
//    public int CurrentHP => currentHP;
//    public int CurrentShield => currentShield;
//    public IReadOnlyList<StatusEffect> StatusEffects => statusEffects;

//    public void Initialize(PlayerData data)
//    {
//        _data = data;
//        currentHP = data.maxHp;
//        currentShield = 0;
//        statusEffects.Clear();
//    }

//    public void Initialize(PlayerData data, int savedHP)
//    {
//        _data = data;
//        currentHP = savedHP;
//        currentShield = 0;
//        statusEffects.Clear();
//    }

//    public int CalculateAttackDamage(int baseDamage)
//        => Mathf.RoundToInt(baseDamage * attackMultiplier);

//    public int TakeDamage(int damage)
//    {
//        int actualDamage = Mathf.Max(0, damage - currentShield);
//        currentShield = Mathf.Max(0, currentShield - damage);
//        currentHP = Mathf.Max(0, currentHP - actualDamage);
//        return actualDamage;
//    }

//    public void TakeDamageRaw(int damage)
//    {
//        currentHP = Mathf.Max(0, currentHP - damage);
//    }

//    public void Heal(int amount)
//    {
//        currentHP += amount;
//        currentHP = currentHP >= MaxHp ? MaxHp : currentHP;
//    }

//    public void ShieldUp(int amount) => currentShield += amount;
//    public void ResetShield() => currentShield = 0;

//    public void ApplyStatusEffect(StatusEffect effect)
//    {
//        var existing = statusEffects.FirstOrDefault(e => e.effectName == effect.effectName);
//        if (existing != null)
//        {
//            existing.duration = Mathf.Max(existing.duration, effect.duration);
//            existing.value = Mathf.Max(existing.value, effect.value);
//        }
//        else
//        {
//            statusEffects.Add(effect);
//        }
//    }

//    public async UniTask ProcessTurnStart(DiceContext ctx)
//    {
//        foreach (var effect in statusEffects)
//            await effect.OnTurnStart(this, ctx);

//        statusEffects.RemoveAll(e => e.Tick());
//    }

//    public bool IsDead() => currentHP <= 0;
//}

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlayerBattleData : IDamageable
{
    private PlayerData _data;
    private BattleEventBus _eventBus;

    private int currentHP;
    private int currentShield;
    private List<StatusEffect> statusEffects = new();
    private float attackMultiplier = 1f;

    public float AttackMultiplier => attackMultiplier;
    public void SetAttackMultiplier(float value) => attackMultiplier = value;
    public void ResetAttackMultiplier() => attackMultiplier = 1f;

    public int MaxHp => _data.maxHp;
    public int CurrentHP => currentHP;
    public int CurrentShield => currentShield;
    public IReadOnlyList<StatusEffect> StatusEffects => statusEffects;

    // NOTE: 이제 Initialize 시점에 eventBus를 받습니다.
    // BattleManager 쪽에서 eventBus를 먼저 만든 뒤 이 Initialize를 호출해야 합니다. (순서 중요)
    public void Initialize(PlayerData data, BattleEventBus eventBus)
    {
        _data = data;
        _eventBus = eventBus;
        currentHP = data.maxHp;
        currentShield = 0;
        statusEffects.Clear();
    }

    public void Initialize(PlayerData data, int savedHP, BattleEventBus eventBus)
    {
        _data = data;
        _eventBus = eventBus;
        currentHP = savedHP;
        currentShield = 0;
        statusEffects.Clear();
    }

    public int CalculateAttackDamage(int baseDamage)
        => Mathf.RoundToInt(baseDamage * attackMultiplier);

    // IDamageable 인터페이스 시그니처(int TakeDamage(int))는 그대로 유지.
    // ctx가 필요한 호출부는 아래 오버로드를 쓰면 됨.
    public int TakeDamage(int damage) => TakeDamage(damage, null);

    public int TakeDamage(int damage, DiceContext ctx)
    {
        int actualDamage = Mathf.Max(0, damage - currentShield);
        currentShield = Mathf.Max(0, currentShield - damage);
        currentHP = Mathf.Max(0, currentHP - actualDamage);

        _eventBus?.TriggerHitPlayer(ctx, actualDamage);
        return actualDamage;
    }

    public void TakeDamageRaw(int damage) => TakeDamageRaw(damage, null);

    public void TakeDamageRaw(int damage, DiceContext ctx)
    {
        currentHP = Mathf.Max(0, currentHP - damage);
        _eventBus?.TriggerHitPlayer(ctx, damage);
    }

    public void Heal(int amount) => Heal(amount, null);

    public void Heal(int amount, DiceContext ctx)
    {
        currentHP += amount;
        currentHP = currentHP >= MaxHp ? MaxHp : currentHP;

        _eventBus?.TriggerPlayerHeal(ctx, amount);
    }

    public void ShieldUp(int amount) => ShieldUp(amount, null);

    public void ShieldUp(int amount, DiceContext ctx)
    {
        currentShield += amount;
        _eventBus?.TriggerPlayerDefend(ctx);
    }

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