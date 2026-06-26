using System.Collections.Generic;

public class DiceContext
{
    public BattleContext battle;
    public int baseDamage;
    public BattleDices dices;
    public DiceState diceState;

    public PlayerBattleData Player => battle.Player;
    public EnemyBattleData Enemy => battle.Enemy;
    public BattleEventBus EventBus => battle.EventBus;
    public bool IsPlayer => battle.IsPlayer;
    public BattlePositions Positions => battle.Positions;
    public System.Threading.CancellationToken CancellationToken => battle.CancellationToken;

    public DiceData DiceData => diceState.diceData;
}

public class BattleDices
{
    public List<Dice> attackDices;
    public List<Dice> defenseDices;
}
