using UnityEngine;
using VContainer;

public class EnemyAI : MonoBehaviour
{
    private BattleDataManager _battleDataManager;

    [Inject]
    public void Construct(BattleDataManager battleDataManager)
    {
        _battleDataManager = battleDataManager;
    }

    public void PlaceDice(Dice[] rolledDice)
    {
        var strategy = _battleDataManager.currentEnemyData.aiStrategy;
        strategy.PlaceDice(rolledDice);
    }
}
