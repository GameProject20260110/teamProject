using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public event Action<int> OnGoldChanged;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnRoundAndGoalChanged;
    public event Action<int> OnRerollCountChanged;

    public DiceManager diceManager;
    public PanelEffect panelEffect;
    public UIFlowController UFC;

    public bool hasUsedPlusReroll = false;
    
    private List<DiceData> _lastDiceDatas;
    private List<int> _lastValues;
    private List<ItemSo> _usedConsumableItems = new List<ItemSo>();
    private int _currentRerollCount;
    private bool _isFirstRoll = true;

    public bool IsFirstRoll => _isFirstRoll;

    public int CurrentRerollCount
    {
        get => _currentRerollCount;
        set
        {
            _currentRerollCount = value;
            OnRerollCountChanged?.Invoke(_currentRerollCount);
        }
    }

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
        if(AudioManager.instance != null)
        {
            AudioManager.instance.PlayBgm(AudioManager.Bgm.Battle, true);
        }
    }

    public void InitializeRoundData()
    {
        _isFirstRoll = PlayerManager.instance.isFirstRoll;
        _currentRerollCount = PlayerManager.instance.gameRerollCount;
        if (UiController.instance != null) UiController.instance.UpdateRerollUi(_currentRerollCount);
        NotifyAllUI();
    }

    public void NotifyAllUI()
    {
        int gold = PlayerManager.instance != null ? PlayerManager.instance.gold : 0;
        int currentRound = RoundManager.instance != null ? RoundManager.instance.currentRound : 0;

        OnGoldChanged?.Invoke(gold);
        OnRoundAndGoalChanged?.Invoke(currentRound);
        OnRerollCountChanged?.Invoke(_currentRerollCount);
    }

    public void AddGold(int amount)
    {
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.gold += amount;
            if (PlayerManager.instance.gold < 0) { PlayerManager.instance.gold = 0; }
            OnGoldChanged?.Invoke(PlayerManager.instance.gold);
        }
    }

    public void StartRound()
    {
        Debug.Log($"{_usedConsumableItems.Count}");
        _usedConsumableItems.Clear();
        if (UiController.instance == null) return;
        _isFirstRoll = PlayerManager.instance.isFirstRoll;
        _currentRerollCount = PlayerManager.instance.gameRerollCount;
        hasUsedPlusReroll = false;

        NotifyAllUI();

        UiController.instance.HideAllPanels();
        UiController.instance.UpdateRerollUi(_currentRerollCount);
        UiController.instance.SetShopBtnInteratable(true);
        UiController.instance.SetRollBtnInteractable(true);
        UiController.instance.SetConfirmBtnInteratable(false);

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
        UiController.instance.SetShopBtnInteratable(false);
        UiController.instance.HideGlowConfirmBtn();
        panelEffect?.HideGlow();

        RollFlow().Forget();
    }

    private async UniTask RollFlow()
    {
        try
        {
            if (_isFirstRoll)
            {
                _isFirstRoll = false;
                PlayerManager.instance.isFirstRoll = false;
                UiController.instance.SetRollButtnonToReroll();
            }
            else
            {
                VisualManager.instance?.ResetDiceColors(diceManager.GetAllDice());
                UiController.instance?.ResetItemCards();
            }

            UiController.instance.UpdateRerollUi(_currentRerollCount);
            Dice[] allDice = await diceManager.StartRolling();

            var result = ScoreManager.instance.CalculateScore(allDice, ScoreManager.DiceType.Roll);

            await UniTask.Delay(500);

            if (VisualManager.instance != null)
            {
                await VisualManager.instance.PlayScoreEventSequence(allDice, result.events);
            }

            ProcessRollResult(result.finalScore, result.consumedItems);

            foreach(var dice in allDice)
            {
                if (dice == null || !dice.gameObject.activeSelf) continue;
                var floatingEffect = dice.GetComponent<FloatingEffect>();
                if (floatingEffect != null) floatingEffect.enabled = true;
            }
            panelEffect?.ShowGlow();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void ProcessRollResult(int finalScore, List<ItemSo> consumedItems)
    {
        _usedConsumableItems = consumedItems;

        if (diceManager != null)
        {
            _lastDiceDatas = new List<DiceData>();
            _lastValues = new List<int>();

            foreach(var dice in diceManager.panelDiceScript)
            {
                if(dice != null && dice.MyState != null && dice.gameObject.activeSelf)
                {
                    _lastDiceDatas.Add(dice.MyState.diceData);
                    _lastValues.Add(dice.MyState.originalValue);
                }
            }
        }

        if (_currentRerollCount <= 0)
        {
            UiController.instance.SetShopBtnInteratable(false);
            UiController.instance.SetRollBtnInteractable(false);
            UiController.instance.SetConfirmBtnInteratable(true);
        }
        else
        {
            UiController.instance.SetShopBtnInteratable(true);
            UiController.instance.SetRollBtnInteractable(true);
            UiController.instance.SetConfirmBtnInteratable(true);
        }
    }

    public void HandleGameOver()
    {

        if(PlayerManager.instance != null)
        {
            PlayerManager.instance.ResetData();
        }

        if(GimmickManager.instance != null)
        {
            GimmickManager.instance.ClearGimmick();
        }

        List<int> fakeValues = new List<int>();
        if (_lastValues != null)
        {
            for (int i = 0; i < _lastValues.Count; i++)
            {
                fakeValues.Add(1);
            }
        }
        UiController.instance.ShowGameOverPanel(RoundManager.instance.currentRound);
    }

    public void OnClickScoreConfirmButton()
    {

        OnClickScoreConfirm().Forget();
    }

    public async UniTask OnClickScoreConfirm()
    {
        if (diceManager.isRolling) return;
        if (BattleManager.instance == null) return;

        List<Dice> attackDices = DicePanelManager.instance.attackPanel.GetDices();
        List<Dice> defenceDices = DicePanelManager.instance.defensePanel.GetDices();

        BattleManager.instance.SetDiceInfo(attackDices, defenceDices);

        // UI 비활성화
        UiController.instance.SetShopBtnInteratable(false);
        UiController.instance.SetConfirmBtnInteratable(false);
        UiController.instance.SetRollBtnInteractable(false);
        UiController.instance.HideGlowConfirmBtn();
        panelEffect?.HideGlow();

        foreach (var dice in diceManager.panelDiceScript)
        {
            if (dice == null || !dice.gameObject.activeSelf) continue;
            var floatingEffect = dice.GetComponent<FloatingEffect>();
            if (floatingEffect != null) floatingEffect.StopFloating();
        }

        // 아이템 처리
        if (_usedConsumableItems != null && _usedConsumableItems.Count > 0)
        {
            RemoveUsedItems(_usedConsumableItems);
        }
        if (PlayerShopManager.instance != null)
        {
            PlayerShopManager.instance.pendingConsumables.Clear();
        }

        try
        {
            await BattleManager.instance.OnPlayerAttack();
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

        _isFirstRoll = true;
        PlayerManager.instance.isFirstRoll = true;
        UiController.instance.SetRollButtnonToReroll();
        UiController.instance.SetShopBtnInteratable(true);
        UiController.instance.SetRollBtnInteractable(true);
    }

    public void OnClickSurrenButton()
    {
        if(_isFirstRoll) return;
        if (PlayerManager.instance == null) return;
        if (RoundManager.instance.currentRound <= 1) return;

        if(PlayerManager.instance != null)
        {
            PlayerManager.instance.ResetData();
        }
        if(GimmickManager.instance != null)
        {
            GimmickManager.instance.ClearGimmick();
        }

        RoundManager.instance.StartRound();
    }

    public void OnClickShopBtn()
    {
        PlayerShopManager.instance.ClearRound = false;
        PlayerShopManager.instance.OpenWithAnimation();
    }

    public void OnClickNextRound()
    {
        if (RoundManager.instance != null)
        {
            RoundManager.instance.GoNextRound();
        }
        PlayerShopManager.instance.ClearRound = true;
        PlayerManager.instance.gameRerollCount = 3;
        PlayerManager.instance.isFirstRoll = true;

        UFC.OnNextRoundButton();
        SceneController.instance?.LoadMapScene();
    }

    public void OnClickRetryRound()
    {
        PlayerManager.instance.gameRerollCount = 3;
        PlayerManager.instance.isFirstRoll = true;
    }

    public void LoadHomeScreen()
    {
        if(PlayerManager.instance != null && !PlayerManager.instance.isGameOver)
        {
            PlayerManager.instance.Save();
        }
        if(PlayerStatsManager.instance != null)
        {
            PlayerStatsManager.instance.RecordGameEnd(RoundManager.instance.currentRound, true);
        }

        SceneManager.LoadScene("HomeScreen");
    }

    private void RemoveUsedItems(List<ItemSo> itemsToRemove)
    {
        if (PlayerManager.instance == null) return;

        var items = PlayerManager.instance.items;
        foreach (var item in itemsToRemove)
        {
            int index = items.IndexOf(item);
            if (index >= 0) items[index] = null;
        }
        UiController.instance.RefreshInventory();
    }
}
    