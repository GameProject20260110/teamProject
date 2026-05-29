using System.Collections.Generic;

public class DiceContextFactory
{
    private readonly BattleContext _battleCtx;

    public DiceContextFactory(BattleContext battleCtx)
    {
        _battleCtx = battleCtx;
    }

    public DiceContext Create(Dice dice, List<Dice> attackDices, List<Dice> defenseDices)
    {
        return new DiceContext
        {
            battle = _battleCtx,
            baseDamage = dice.MyState.originalValue,
            diceData = dice.MyState.diceData,
            dices = new BattleDices
            {
                attackDices = attackDices,
                defenseDices = defenseDices
            }
        };
    }

    // 주사위 없이 이벤트 트리거용 (턴 시작, 적 사망 등)
    public DiceContext CreateEmpty()
    {
        return new DiceContext
        {
            battle = _battleCtx,
            baseDamage = 0,
            diceData = null,
            dices = null
        };
    }
}