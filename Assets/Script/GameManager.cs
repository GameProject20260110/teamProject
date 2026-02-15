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

    public bool hasShoes = false;
    public DiceAbilitys dices;
    public DiceManager diceManager;
    public int maxRerollCount = 1;
    public int currentScore = 0;
    public int bestScore = 0;
    public bool hasUsedPlusReroll = false;

    private List<DiceData> _lastDiceDatas;
    private List<int> _lastValues;
    private bool _isFirstRoll = true;
    private int _currentRerollCount;

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartRound();
        UiController.instance.SetRollBtnInteractable(true);
        UiController.instance.SetConfirmBtnInteratable(false);
    }
    
    public void NotifyAllUI()
    {
        int currentGoldVal = (playerData != null) ? playerData.gold : gold;
        OnGoldChanged?.Invoke(currentGoldVal);
        OnScoreChanged?.Invoke(currentScore);
        OnLivesChanged?.Invoke(currentLives);
        OnRoundAndGoalChanged?.Invoke(currentRound, targetScore);
        OnRerollCountChanged?.Invoke(_currentRerollCount);
    }

    public void AddGold(int gold)
    {
        if (playerData != null)
        {
            playerData.gold += gold;
            if (playerData.gold < 0) { playerData.gold = 0; }
            OnGoldChanged?.Invoke(playerData.gold);
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
        UiController.instance.UPdateRerollUi(_currentRerollCount);
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

        UiController.instance.SetRollBtnInteractable(false);
        UiController.instance.rollBtn.interactable = false;

        for (int i = 0; i < diceManager.panelDiceScript.Length; i++)
        {
            diceManager.panelDiceScript[i].MyState.diceData.multiBonusScore = 1;
            diceManager.panelDiceScript[i].MyState.diceData.plusBonusScore = 0;
        }

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

        UiController.instance.UPdateRerollUi(_currentRerollCount);
        if(!_isFirstRoll && _currentRerollCount <= 0)
        {
            UiController.instance.SetRollBtnInteractable(false);
        }
    }

    public void OnClickScoreConfirm()
    {
        if (UiController.instance.rollBtn.interactable == false && diceManager.isRolling) return;
        Debug.Log("점수 확정 버튼 클릭");
        CompleteRound();    
    }

    public void ProcessRollResult(int finalScore)
    {
        currentScore = finalScore;
        OnScoreChanged?.Invoke(currentScore);

        if (diceManager != null)
        {
            _lastDiceDatas = new List<DiceData>();
            _lastValues = new List<int>();

            foreach(var dice in diceManager.panelDiceScript)
            {
                if(dice != null && dice.MyState != null)
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

    public void CompleteRound()
    {
        UiController.instance.SetRollBtnInteractable(false);

        bool isSuccess = currentScore >= targetScore;

        if (isSuccess)
        {
            UiController.instance.ShowResultPanel(true, targetScore, currentScore, currentLives);
        }
        else
        {
            ModifyLives(-1);
            if (currentLives > 0)
            {
                UiController.instance.ShowResultPanel(false, targetScore, currentScore, currentLives);
            }
            else
            {
                // 게임 오버시 모든 눈금을 1로 채워서 보여주기 위해 사용
                List<int> fakeValue = new List<int>();
                if(_lastValues != null)
                {
                    for(int i = 0; i < _lastValues.Count; i++)
                    {
                        fakeValue.Add(1);
                    }
                }
                UiController.instance.ShowGameOverPanel(currentRound, bestScore, _lastDiceDatas, fakeValue);
            }
        }
    }

    public void OnClickNextRound()
    {
        Debug.Log("다음 라운드로 이동~");
        // 라운드 이동 처리 필요
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
}
    