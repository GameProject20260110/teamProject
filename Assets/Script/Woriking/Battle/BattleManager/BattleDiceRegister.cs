using System.Collections.Generic;

public class BattleDiceRegistry
{
    private List<Dice> _attackDices = new List<Dice>();
    private List<Dice> _defenseDices = new List<Dice>();
    private List<Dice> _attackEnemyDices = new List<Dice>();
    private List<Dice> _defenseEnemyDices = new List<Dice>();

    public List<Dice> AttackDices => _attackDices;
    public List<Dice> DefenseDices => _defenseDices;
    public List<Dice> AttackEnemyDices => _attackEnemyDices;
    public List<Dice> DefenseEnemyDices => _defenseEnemyDices;

    public void SetPlayerDice(List<Dice> attackDices, List<Dice> defenseDices)
    {
        _attackDices = attackDices;
        _defenseDices = defenseDices;
    }

    public void AddEnemyAttackDice(Dice dice) => _attackEnemyDices.Add(dice);
    public void AddEnemyDefenseDice(Dice dice) => _defenseEnemyDices.Add(dice);

    public void ClearEnemyDices()
    {
        _attackEnemyDices.Clear();
        _defenseEnemyDices.Clear();
    }

    public void ResetAllVFX()
    {
        foreach (var dice in _attackDices) dice.VFX?.ResetBuff();
        foreach (var dice in _defenseDices) dice.VFX?.ResetBuff();
        foreach (var dice in _attackEnemyDices) dice.VFX?.ResetBuff();
        foreach (var dice in _defenseEnemyDices) dice.VFX?.ResetBuff();
    }
}