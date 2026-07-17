using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool hasUsedPlusReroll = false;

    private DiceManager _diceManager;
    private ResourceManager _resourceManager;
    private BattleButton _battleButton;
    private EnemyAI _enemyAI;
    private BattleManager _battleManager;
    private DicePanelManager _dicePanelManager;

    [Inject]
    public void Construct(
        DiceManager diceManager,
        ResourceManager resourceManager,
        BattleButton battleButton,
        EnemyAI enemyAI,
        BattleManager battleManager,
        DicePanelManager dicePanelManager)
    {
        _diceManager = diceManager;
        _resourceManager = resourceManager;
        _battleButton = battleButton;
        _enemyAI = enemyAI;
        _battleManager = battleManager;
        _dicePanelManager = dicePanelManager;
        Instance = this;
    }

    
    public void OnClickRollBtn()
    {
        RollFlow().Forget();
    }

    private async UniTask RollFlow()
    {
        try
        {
            _battleButton.SetInteractable(false);
            UiController.Instance?.ResetItemCards();

            Dice[] allDice = await _diceManager.StartRolling();
            foreach (var dice in allDice)
            {
                if (dice == null || !dice.gameObject.activeSelf) continue;
                var floatingEffect = dice.GetComponentInChildren<FloatingEffect>();
                dice.GetComponent<DraggableDice>().SetDraggable(true);
                if (floatingEffect != null) floatingEffect.enabled = true;
            }
            await _battleButton.SetState(BattleButton.State.PlaceComplete);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void HandleGameOver()
    {
        if (_resourceManager != null)
            _resourceManager.ResetData();

        UiController.Instance.ShowGameOverPanel(false);
    }

    public void OnClickScoreConfirmButton()
    {
        OnClickScoreConfirm().Forget();
    }

    public async UniTask EnemyRoll()
    {
        await _battleButton.SetState(BattleButton.State.EnemyTurn);
        Dice[] allDice = await _diceManager.StartEnemyRolling();
        await UniTask.Delay(500);
        _enemyAI.PlaceDice(allDice);
        await _battleButton.SetState(BattleButton.State.Roll);
    }

    public async UniTask OnClickScoreConfirm()
    {
        if (_diceManager.isRolling) return;
        if (_battleManager == null) return;

        await _battleButton.SetState(BattleButton.State.InBattle);

        List<Dice> attackDices = _dicePanelManager.attackPanel.GetDices();
        List<Dice> defenceDices = _dicePanelManager.defensePanel.GetDices();

        _battleManager.SetDiceInfo(attackDices, defenceDices);

        foreach (var dice in _diceManager.panelDiceScript)
        {
            if (dice == null || !dice.gameObject.activeSelf) continue;
            var floatingEffect = dice.GetComponentInChildren<FloatingEffect>();
            dice.GetComponent<DraggableDice>().SetDraggable(false);
            if (floatingEffect != null) floatingEffect.StopFloating();
        }

        try
        {
            await _battleManager.RunOneTurnCycle();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("¿¸≈ı √Îº“µ ");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}