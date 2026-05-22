using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlayerBattleData : IDamageable
{
    private PlayerData _data;
    private int currentHP;
    private int currentShield;
    private int _bonusAttack;   // 주사위 합산 공격력

    private List<StatusEffect> statusEffects = new();

    // 프로퍼티
    public int MaxHp => _data.maxHp;
    public int CurrentHP => currentHP;
    public int CurrentShield => currentShield;
    public IReadOnlyList<StatusEffect> StatusEffects => statusEffects;

    public int AttackPower => _data.baseAttack + _bonusAttack;

    // 초기화
    public void Initialize(PlayerData data)
    {
        _data = data;
        currentHP = data.maxHp;
        currentShield = 0;
        _bonusAttack = 0;
        statusEffects.Clear();
    }

    public void Initialize(PlayerData data, int savedHP)
    {
        _data = data;
        currentHP = savedHP;
        currentShield = 0;
        _bonusAttack = 0;
        statusEffects.Clear();
    }

    // 주사위 결과로 스탯 세팅
    public void SetBattleStats(int bonusAttack, int shield)
    {
        _bonusAttack = bonusAttack;
        currentShield = shield;
    }

    // 일반 데미지 (쉴드 적용)
    public int TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(0, damage - currentShield);
        currentShield = Mathf.Max(0, currentShield - damage);
        currentHP = Mathf.Max(0, currentHP - actualDamage);
        return actualDamage;
    }

    // 쉴드 무시 데미지 (번, 독 등)
    public void TakeDamageRaw(int damage)
    {
        currentHP = Mathf.Max(0, currentHP - damage);
    }

    public void ShieldUp(int amount) => currentShield += amount;
    public void ResetShield() => currentShield = 0;

    // 상태이상 추가 (같은 종류면 갱신)
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

    // 턴 시작마다 호출
    //public void ProcessTurnStart(Vector3 position)
    //{
    //    foreach (var effect in statusEffects)
    //        effect.OnTurnStart(this, ctx);

    //    // 만료된 효과 제거
    //    statusEffects.RemoveAll(e => e.Tick());
    //}

    public bool IsDead() => currentHP <= 0;
}
