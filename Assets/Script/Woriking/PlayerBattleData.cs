using UnityEngine;

[System.Serializable]
public class PlayerBattleData
{
    private int maxHp = 50;
    private int currentShield;
    private int currentHP;
    private int attackPower;

    public int MaxHp => maxHp;
    public int CurrentHP => currentHP;
    public int CurrentShield => currentShield;
    public int AttackPower => attackPower;
    public void Initialize()
    {
        this.currentHP = maxHp;
        this.currentShield = 0;
    }

    public void Initialize(int currentHP)
    {
        this.currentHP = currentHP;
        this.currentShield = 0;
    }

    public void SetPlayerStats(int attackPower, int defensePower)
    {
        this.attackPower = attackPower;
        ShieldUp(defensePower);
    }

    public void TakeDamage(int damage)
    {
        
        int actualDamage = (damage - currentShield) <= 0 ? 0 : damage - currentShield;
        currentShield = (currentShield - damage) <= 0 ? 0 : currentShield - damage;
        

        currentHP -= actualDamage;
        currentHP = Mathf.Max(0, currentHP);
    }

    public void ShieldUp(int amount)
    {
        currentShield = amount;
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }

}
