using UnityEngine;

public abstract class EnemyAIStrategy : ScriptableObject
{
    public abstract void PlaceDice(Dice[] rolledDice);

    // 공통 유틸 — 상속받은 AI들이 공통으로 쓸 수 있게
    protected void PlaceByRole(Dice[] rolledDice)
    {
        int attackSlot = 0;
        int defenseSlot = 0;

        foreach (var dice in rolledDice)
        {
            if (dice == null) continue;

            switch (dice.MyState.diceData.aiRole)
            {
                case DiceData.DiceRole.Attack:
                    DiceManager.instance.EnemyPlaceAttackDice(attackSlot++, dice);
                    break;
                case DiceData.DiceRole.Defense:
                    DiceManager.instance.EnemyPlaceDefenseDice(defenseSlot++, dice);
                    break;
                case DiceData.DiceRole.Neutral:
                    if (attackSlot <= defenseSlot)
                        DiceManager.instance.EnemyPlaceAttackDice(attackSlot++, dice);
                    else
                        DiceManager.instance.EnemyPlaceDefenseDice(defenseSlot++, dice);
                    break;
            }
        }
    }
}
