using UnityEngine;

[System.Serializable]
public class EnemyBattleData
{
    private int maxHp = 50;
    private int currentHP;

    public int MaxHp => maxHp;
    public int CurrentHP => currentHP;

    public void Initialize()
    {
        this.currentHP = maxHp;
    }

    public void Initialize(int currentHP)
    {
        this.maxHp = currentHP;
        this.currentHP = currentHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }
}