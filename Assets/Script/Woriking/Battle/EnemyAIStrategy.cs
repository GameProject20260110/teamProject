using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class EnemyAIStrategy : ScriptableObject
{
    public abstract UniTask PlaceDice(Dice[] rolledDice);

    protected async UniTask PlaceByRole(Dice[] rolledDice)
    {
        int attackSlot = 0;
        int defenseSlot = 0;
        foreach (var dice in rolledDice)
        {
            if (dice == null) continue;
            switch (dice.MyState.diceData.aiRole)
            {
                case DiceData.DiceRole.Attack:
                    await DiceManager.instance.EnemyPlaceAttackDice(attackSlot++, dice);
                    break;
                case DiceData.DiceRole.Defense:
                    await DiceManager.instance.EnemyPlaceDefenseDice(defenseSlot++, dice);
                    break;
                case DiceData.DiceRole.Neutral:
                    if (attackSlot <= defenseSlot)
                        await DiceManager.instance.EnemyPlaceAttackDice(attackSlot++, dice);
                    else
                        await DiceManager.instance.EnemyPlaceDefenseDice(defenseSlot++, dice);
                    break;
            }
        }
    }
}
