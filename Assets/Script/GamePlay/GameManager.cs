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
    [SerializeField] private AudioClip BattleBGM;

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
        AudioManager.instance.PlayBgm(BattleBGM);
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
        {
            EnemyDeckHandler.instance.SetupEnemyDice();
        }

        if (DeckManager.instance != null)
        {
            DeckManager.instance.DrawDice();
        }

    }

    public void OnClickRollBtn()
    {

        if (UiController.instance.rollBtn.interactable == false) return;
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Roll);
        UiController.instance.SetRollBtnInteractable(false);
        UiController.instance.SetConfirmBtnInteratable(false);
        UiController.instance.rollBtn.interactable = false;
        UiController.instance.HideGlowConfirmBtn();

        RollFlow().Forget();
    }

    private async UniTask RollFlow()
    {
        try
        {
            
            
            UiController.instance?.ResetItemCards();
            

            Dice[] allDice = await diceManager.StartRolling();

            ProcessRollResult();

            foreach(var dice in allDice)
            {
                if (dice == null || !dice.gameObject.activeSelf) continue;
                var floatingEffect = dice.GetComponent<FloatingEffect>();
                if (floatingEffect != null) floatingEffect.enabled = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void ProcessRollResult()
    {
        UiController.instance.SetRollBtnInteractable(true);
        UiController.instance.SetConfirmBtnInteratable(true);       
    }

    public void HandleGameOver()
    {

        if(ResourceManager.instance != null)
        {
            ResourceManager.instance.ResetData();
        }
        UiController.instance.ShowGameOverPanel(1);
    }

    public void OnClickScoreConfirmButton()
    {
        OnClickScoreConfirm().Forget();
    }

    public async UniTask EnemyRoll()
    {
        Dice[] allDice = await diceManager.StartEnemyRolling();

        await UniTask.Delay(500);

        ProcessRollResult();

        EnemyAI.instance.PlaceDice(allDice);
    }

    public async UniTask OnClickScoreConfirm()
    {
        if (diceManager.isRolling) return;
        if (BattleManager.instance == null) return;

        List<Dice> attackDices = DicePanelManager.instance.attackPanel.GetDices();
        List<Dice> defenceDices = DicePanelManager.instance.defensePanel.GetDices();

        BattleManager.instance.SetDiceInfo(attackDices, defenceDices);

        // UI 비활성화
        UiController.instance.SetConfirmBtnInteratable(false);
        UiController.instance.SetRollBtnInteractable(false);
        UiController.instance.HideGlowConfirmBtn();

        foreach (var dice in diceManager.panelDiceScript)
        {
            if (dice == null || !dice.gameObject.activeSelf) continue;
            var floatingEffect = dice.GetComponent<FloatingEffect>();
            if (floatingEffect != null) floatingEffect.StopFloating();
        }

        try
        {
            await BattleManager.instance.EnemyDefense();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("전투 취소됨");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        DicePanelManager.instance?.ResetAllDice(diceManager.GetAllDice());

        UiController.instance.SetRollBtnInteractable(true);
    }
}
    