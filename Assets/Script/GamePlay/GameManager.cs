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
    public event Action<int> OnHeartsChanged;
    public event Action<int, int> OnRoundAndGoalChanged;
    public event Action<int> OnRerollCountChanged;

    public DiceManager diceManager;
    public UIFlowController UFC;
    public int maxRerollCount = 1;
    public int currentScore = 0;
    public int bestScore = 0;
    public bool hasUsedPlusReroll = false;

    private List<DiceData> _lastDiceDatas;
    private List<int> _lastValues;
    private List<ItemSo> _usedConsumableItems = new List<ItemSo>();
    private bool _isFirstRoll = true;
    private int _currentRerollCount;
    private int _fallbackHeart;

    public int CurrentHearts
    {
        get => PlayerManager.instance != null ? PlayerManager.instance.heart : _fallbackHeart;
        private set
        {
            if (PlayerManager.instance != null)
            {
                PlayerManager.instance.heart = value;
            }
            else
                _fallbackHeart = value;
        }
    }

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
        currentScore = 0;
        if (UiController.instance != null) UiController.instance.UpdateRerollUi(_currentRerollCount);
        NotifyAllUI();
    }

    public void NotifyAllUI()
    {
        int gold = PlayerManager.instance != null ? PlayerManager.instance.gold : 0;
        int currentRound = RoundManager.instance != null ? RoundManager.instance.currentRound : 0;
        int targetScore = RoundManager.instance != null ? RoundManager.instance.targetScore : 0;

        OnGoldChanged?.Invoke(gold);
        OnScoreChanged?.Invoke(currentScore);
        OnHeartsChanged?.Invoke(CurrentHearts);
        OnRoundAndGoalChanged?.Invoke(currentRound, targetScore);
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

    public void ModifyHearts(int heart)
    {
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.heart += heart;
            if (PlayerManager.instance.heart < 0) PlayerManager.instance.heart = 0;
            PlayerManager.instance.Save();
        }
        else
        {
            _fallbackHeart += heart;
            if(_fallbackHeart < 0) _fallbackHeart = 0;
        }
        OnHeartsChanged?.Invoke(CurrentHearts);

    }

    public void StartRound()
    {
        Debug.Log($"{_usedConsumableItems.Count}");
        _usedConsumableItems.Clear();
        if (UiController.instance == null) return;
        _isFirstRoll = PlayerManager.instance.isFirstRoll;
        _currentRerollCount = PlayerManager.instance.gameRerollCount;
        currentScore = 0;
        hasUsedPlusReroll = false;

        NotifyAllUI();

        UiController.instance.HideAllPanels();
        UiController.instance.UpdateRerollUi(_currentRerollCount);
        UiController.instance.SetShopBtnInteratable(true);
        UiController.instance.SetRollBtnInteractable(true);
        UiController.instance.SetConfirmBtnInteratable(false);

        if (diceManager != null)
        {
            diceManager.SetupDiceBoard();
        }

        foreach(var dice in diceManager.panelDiceScript)
        {
            if(dice != null && dice.gameObject.activeSelf)
            {
                dice.UpdateDiceScoreUi(0, hide: true);
            }
        }

    }

    public void OnClickRollBtn()
    {

        if (UiController.instance.rollBtn.interactable == false) return;
        if(AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Roll);
        UiController.instance.SetRollBtnInteractable(false);
        UiController.instance.SetConfirmBtnInteratable(false);
        UiController.instance.rollBtn.interactable = false;
        UiController.instance.SetShopBtnInteratable(false);

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
                currentScore = 0;
                ScoreVisualizer.instance?.UpdateScoreBoard(0);
                ScoreVisualizer.instance?.ClearNegateOverlays();
                ScoreVisualizer.instance?.ResetDiceColors(diceManager.GetAllDice());
                UiController.instance?.ResetItemCards();
            }

            UiController.instance.UpdateRerollUi(_currentRerollCount);
            Dice[] allDice = await diceManager.StartRolling();

            var result = ScoreManager.instance.CalculateScore(allDice, ScoreManager.DiceType.Roll);

            ScoreVisualizer.instance?.UpdateScoreBoard(result.baseScore);
            foreach(var dice in allDice)
            {
                if (dice == null || !dice.gameObject.activeSelf) continue;
                if (dice.MyState == null) continue;
                dice.UpdateDiceScoreUi(dice.MyState.originalValue, false);
            }
            await UniTask.Delay(500);

            if (ScoreVisualizer.instance != null)
            {
                await ScoreVisualizer.instance.PlayScoreEventSequence(allDice, result.events);
            }

            ProcessRollResult(result.finalScore, result.consumedItems);

            await BattleManager.instance.OnPlayerAttack(currentScore);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void ProcessRollResult(int finalScore, List<ItemSo> consumedItems)
    {
        currentScore = finalScore;
        _usedConsumableItems = consumedItems;
        OnScoreChanged?.Invoke(currentScore);

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

        if(currentScore > bestScore)
        {
            bestScore = currentScore;
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
        UiController.instance.ShowGameOverPanel(RoundManager.instance.currentRound, bestScore, _lastDiceDatas, fakeValues);
    }

    public void OnClickScoreConfirm()
    {
        if (diceManager.isRolling) return;

        UiController.instance.SetShopBtnInteratable(false);
        UiController.instance.SetConfirmBtnInteratable(false);
        UiController.instance.SetRollBtnInteractable(false);

        if(_usedConsumableItems != null && _usedConsumableItems.Count > 0)
        {
            RemoveUsedItems(_usedConsumableItems);
        }
        if(PlayerShopManager.instance != null)
        {
            PlayerShopManager.instance.pendingConsumables.Clear();
        }

        if (RoundManager.instance != null) RoundManager.instance.CompleteRound(currentScore);
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
        PlayerManager.instance.gameRerollCount = 1;
        PlayerManager.instance.isFirstRoll = true;

        UFC.OnNextRoundButton();
    }

    public void OnClickRetryRound()
    {
        PlayerManager.instance.gameRerollCount = 1;
        PlayerManager.instance.isFirstRoll = true;
        PlayerShopManager.instance.OpenWithAnimation();
    }

    public void LoadHomeScreen()
    {
        if(PlayerManager.instance != null && !PlayerManager.instance.isGameOver)
        {
            PlayerManager.instance.Save();
        }
        if(PlayerStatsManager.instance != null)
        {
            PlayerStatsManager.instance.RecordGameEnd(RoundManager.instance.currentRound, currentScore, false);
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
    