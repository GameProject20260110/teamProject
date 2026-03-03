using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PlayerSo playerData;

    public event Action<int> OnGoldChanged;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnLivesChanged;
    public event Action<int, int> OnRoundAndGoalChanged;
    public event Action<int> OnRerollCountChanged;

    [Header("테스트용 설정")]
    public int currentRound = 1;
    public int targetScore = 20;
    public int maxLives = 3;
    public int currentLives;
    public int heart = 3;
    public int gold = 50;

    public DiceManager diceManager;
    public int maxRerollCount = 1;
    public int currentScore = 0;
    public int bestScore = 0;
    public bool hasUsedPlusReroll = false;

    private bool useTestMode = TestModeManager.instance != null && TestModeManager.instance.isTestModeActive;

    private List<DiceData> _lastDiceDatas;
    private List<int> _lastValues;
    private bool _isFirstRoll = true;
    private int _currentRerollCount;
    private int _finalCalculateScore = 0;
    private List<ScoreEventData> _scoreEvents = new List<ScoreEventData>();
    private List<ItemSo> _usedConsumableItems = new List<ItemSo>();

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
            //DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        currentLives = maxLives;
    }

    public void InitializeRoundData()
    {
        _isFirstRoll = true;
        _currentRerollCount = maxRerollCount;
        currentScore = 0;
        if (UiController.instance != null) UiController.instance.UpdateRerollUi(_currentRerollCount);
    }

    public void NotifyAllUI()
    {
        int currentGoldVal = (PlayerManager.instance != null) ? PlayerManager.instance.gold : 0;
        OnGoldChanged?.Invoke(currentGoldVal);

        OnScoreChanged?.Invoke(currentScore);
        OnLivesChanged?.Invoke(currentLives);
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
        else
        {
            this.gold += gold;
            OnGoldChanged?.Invoke(gold);
        }
    }

    public void ModifyLives(int lives)
    {
        currentLives += lives;
        OnLivesChanged?.Invoke(currentLives);
    }

    public void StartRound()
    {
        if (UiController.instance == null) return;

        _isFirstRoll = true;
        _currentRerollCount = maxRerollCount;
        currentScore = 0;
        hasUsedPlusReroll = false;

        NotifyAllUI();

        UiController.instance.HideAllPanels();
        UiController.instance.UpdateRerollUi(_currentRerollCount);
        UiController.instance.SetRollBtnInteractable(true);
        UiController.instance.SetConfirmBtnInteratable(false);

        if (diceManager != null)
        {
            diceManager.SetupDiceBoard();
        }

    }

    public void OnClickRollBtn()
    {

        if (UiController.instance.rollBtn.interactable == false) return;
        if(AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Roll);
        UiController.instance.SetRollBtnInteractable(false);
        UiController.instance.rollBtn.interactable = false;

        //for (int i = 0; i < diceManager.panelDiceScript.Length; i++)
        //{
        //    diceManager.panelDiceScript[i].MyState.diceData.multiBonusScore = 1;
        //    diceManager.panelDiceScript[i].MyState.diceData.plusBonusScore = 0;
        //}

        if (_isFirstRoll)
        {
            _isFirstRoll = false;
            diceManager.StartRolling();
            Debug.Log("첫번째 굴리기");
        }
        else if(!_isFirstRoll && _currentRerollCount > 0)
        {
            _currentRerollCount--;
            diceManager.StartRolling();
            Debug.Log("다시 굴리기");
        }

        UiController.instance.UpdateRerollUi(_currentRerollCount);
        if(!_isFirstRoll && _currentRerollCount <= 0)
        {
            UiController.instance.SetRollBtnInteractable(false);
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

        // 최고 점수 갱신
        if(currentScore > bestScore)
        {
            bestScore = currentScore;
        }

        if (_currentRerollCount <= 0)
        {
            Debug.Log("리롤 횟수 소진");
            UiController.instance.SetRollBtnInteractable(false);
            UiController.instance.SetConfirmBtnInteratable(true);
        }
        else
        {
            UiController.instance.SetRollBtnInteractable(true);
            UiController.instance.SetConfirmBtnInteratable(true);
        }
    }
    
    public void OnClickScoreConfirm()
    {
        if (diceManager.isRolling) return;

        UiController.instance.SetConfirmBtnInteratable(false);
        UiController.instance.SetRollBtnInteractable(false);

        if(_usedConsumableItems != null && _usedConsumableItems.Count > 0)
        {
            RemoveUsedItems(_usedConsumableItems);
        }

        if (RoundManager.instance != null) RoundManager.instance.CompleteRound(currentScore);
    }

    public void HandleGameOver()
    {
        Debug.Log("게임 오버 처리");
        List<int> fakeValues = new List<int>();
        if(_lastValues != null)
        {
            for(int i = 0; i < _lastValues.Count; i++)
            {
                fakeValues.Add(1);
            }
        }
        UiController.instance.ShowGameOverPanel(currentRound, bestScore, _lastDiceDatas, fakeValues);
    }

    public void OnClickNextRound()
    {
        Debug.Log("다음 라운드로 이동~");
        if(RoundManager.instance != null)
        {
            RoundManager.instance.GoNextRound();
        }
    }

    public void LoadHomeScreen()
    {
        SceneManager.LoadScene("HomeScreen");
    }

    public void LoadShopScreen()    
    {
        SceneManager.LoadScene("Shop");
    }

    public void LoadGameScreen()
    {
        SceneManager.LoadScene("GameBoard");
    }

    public void LoadSelectScreen()
    {
        SceneManager.LoadScene("DiceSelect");
    }

    private void RemoveUsedItems(List<ItemSo> itemsToRemove)
    {
        if (playerData == null || playerData.itemSo == null) return;

        foreach (var item in itemsToRemove)
        {
            if(playerData.itemSo.Contains(item))
            {
                playerData.itemSo.Remove(item);
            }
        }
        // UI 인벤토리 갱신 필요
    }
}
    