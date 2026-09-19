using UnityEngine;
using VContainer;

public class EnemyDeckHandler : MonoBehaviour
{
    private BattleDataManager _battleDataManager;
    private DiceManager _diceManager;
    private DiceSpawnAnimation _diceSpawnAnimation;

    [Inject]
    public void Construct(BattleDataManager battleDataManager, DiceManager diceManager, DiceSpawnAnimation diceSpawnAnimation)
    {
        _battleDataManager = battleDataManager;
        _diceManager = diceManager;
        _diceSpawnAnimation = diceSpawnAnimation;
    }

    public void SetupEnemyDice()
    {
        var baseData = _battleDataManager.currentEnemyData;
        _diceManager.ClearEnemyAllSlots();
        _diceSpawnAnimation.ClearEnemyList();
        // 일반 몹
        if (baseData is EnemyData enemyData)
        {
            for (int i = 0; i < enemyData.dicePool.Length; i++)
            {
                _diceManager.EnemyPlaceDice(i, enemyData.dicePool[i]);
                var dice = _diceManager.enemyPanelDiceScript[i];
                if (dice != null)
                {
                    var particle = dice.GetComponentInChildren<ParticleSystem>();
                    _diceSpawnAnimation.RegisterEnemyDice(dice.gameObject, particle);
                }
            }
        }
        // 보스
        else if (baseData is BossDataSo bossData)
        {
            for (int i = 0; i < bossData.dicePool.Length; i++)
            {
                _diceManager.EnemyPlaceDice(i, bossData.dicePool[i]);
                var dice = _diceManager.enemyPanelDiceScript[i];
                if (dice != null)
                {
                    var particle = dice.GetComponentInChildren<ParticleSystem>();
                    _diceSpawnAnimation.RegisterEnemyDice(dice.gameObject, particle);
                }
            }
        }
    }
}
