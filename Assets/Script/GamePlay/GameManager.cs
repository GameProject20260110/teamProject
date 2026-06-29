using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public event Action<int> OnGoldChanged;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnRoundAndGoalChanged;
    public event Action<int> OnRerollCountChanged;

    public DiceManager diceManager;
    //[SerializeField] private string BattleBgmKey;

    public bool hasUsedPlusReroll = false;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        NotifyAllUI();
        AudioManager.instance.PlayBgm("Battle",true);
    }

    public void InitializeRoundData()
    {
        NotifyAllUI();
    }

    public void NotifyAllUI()
    {
        int gold = ResourceManager.instance != null ? ResourceManager.instance.gold : 0;
        int currentRound = 0;

        OnGoldChanged?.Invoke(gold);
        OnRoundAndGoalChanged?.Invoke(currentRound);
    }

    public void StartRound()
    {
        if (UiController.instance == null) return;
        hasUsedPlusReroll = false;

        NotifyAllUI();

        UiController.instance.HideAllPanels();
        UiController.instance.SetRollBtnInteractable(true);
        UiController.instance.SetConfirmBtnInteratable(false);

        if (EnemyDeckHandler.instance != null)
            EnemyDeckHandler.instance.SetupEnemyDice();
        
        if (DeckManager.instance != null)
            DeckManager.instance.DrawDice();       
    }

    public void OnClickRollBtn()
    {

        if (UiController.instance.rollBtn.interactable == false) return;
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx("Roll");

        RollFlow().Forget();
    }

    private async UniTask RollFlow()
    {
        try
        {
            BattleButton.instance.SetInteractable(false);
            UiController.instance?.ResetItemCards();
            
            Dice[] allDice = await diceManager.StartRolling();

            foreach(var dice in allDice)
            {
                if (dice == null || !dice.gameObject.activeSelf) continue;
                var floatingEffect = dice.GetComponent<FloatingEffect>();
                dice.GetComponent<DraggableDice>().SetDraggable(true);
                if (floatingEffect != null) floatingEffect.enabled = true;
            }

            await BattleButton.instance.SetState(BattleButton.State.PlaceComplete);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void HandleGameOver()
    {
        if(ResourceManager.instance != null)
            ResourceManager.instance.ResetData();
        
        UiController.instance.ShowGameOverPanel(1);
    }

    public void OnClickScoreConfirmButton()
    {
        OnClickScoreConfirm().Forget();
    }

    public async UniTask EnemyRoll()
    {
        await BattleButton.instance.SetState(BattleButton.State.EnemyTurn);

        Dice[] allDice = await diceManager.StartEnemyRolling();

        await UniTask.Delay(500);

        EnemyAI.instance.PlaceDice(allDice);

        await BattleButton.instance.SetState(BattleButton.State.Roll);
        UiController.instance.SetRollBtnInteractable(true);
    }

    public async UniTask OnClickScoreConfirm()
    {
        if (diceManager.isRolling) return;
        if (BattleManager.instance == null) return;

        await BattleButton.instance.SetState(BattleButton.State.InBattle);

        List<Dice> attackDices = DicePanelManager.instance.attackPanel.GetDices();
        List<Dice> defenceDices = DicePanelManager.instance.defensePanel.GetDices();

        BattleManager.instance.SetDiceInfo(attackDices, defenceDices);

        foreach (var dice in diceManager.panelDiceScript)
        {
            if (dice == null || !dice.gameObject.activeSelf) continue;
            var floatingEffect = dice.GetComponent<FloatingEffect>();
            dice.GetComponent<DraggableDice>().SetDraggable(false);
            if (floatingEffect != null) floatingEffect.StopFloating();
        }

        try
        {
            await BattleManager.instance.RunOneTurnCycle();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("¿¸≈ı √Îº“µ ");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        DicePanelManager.instance?.ResetAllDice(diceManager.GetAllDice());
    }
}
    