using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.InputSystem.Controls;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public event Action<int> OnGoldChanged;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnHeartsChanged;
    public event Action<int, int> OnRoundAndGoalChanged;
    public event Action<int> OnRerollCountChanged;

    public DiceManager diceManager;
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
        AudioManager.instance.PlayBgm(AudioManager.Bgm.Battle,true);
    }

    public void InitializeRoundData()
    {
        _isFirstRoll = true;
        _currentRerollCount = maxRerollCount;
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


        if (_isFirstRoll)
        {
            _isFirstRoll = false;
            diceManager.StartRolling();
            Debug.Log("ù��° ������");
        }
        else if(!_isFirstRoll && _currentRerollCount > 0)
        {
            _currentRerollCount--;
            diceManager.StartRolling();
            Debug.Log("�ٽ� ������");
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

        // �ְ� ���� ����
        if(currentScore > bestScore)
        {
            bestScore = currentScore;
        }

        if (_currentRerollCount <= 0)
        {
            Debug.Log("���� Ƚ�� ����");
            UiController.instance.SetRollBtnInteractable(false);
            UiController.instance.SetConfirmBtnInteratable(true);
        }
        else
        {
            UiController.instance.SetRollBtnInteractable(true);
            UiController.instance.SetConfirmBtnInteratable(true);
        }
    }

    public void HandleGameOver()
    {
        Debug.Log("���� ���� ó��");

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

        UiController.instance.SetConfirmBtnInteratable(false);
        UiController.instance.SetRollBtnInteractable(false);

        if(_usedConsumableItems != null && _usedConsumableItems.Count > 0)
        {
            RemoveUsedItems(_usedConsumableItems);
        }

        if (RoundManager.instance != null) RoundManager.instance.CompleteRound(currentScore);
    }

    public void OnClickSurrenButton()
    {
        if(_isFirstRoll) return;

        if(RoundManager.instance != null && RoundManager.instance.currentRound > 1)
        {
            PlayerManager.instance.ResetData();
        }
        RoundManager.instance.StartRound();
    }

    public void OnClickNextRound()
    {
        Debug.Log("���� ����� �̵�~");
        if(RoundManager.instance != null)
        {
            RoundManager.instance.GoNextRound();
        }
    }

    public void LoadHomeScreen()
    {
        if(PlayerManager.instance != null && !PlayerManager.instance.isGameOver)
        {
            PlayerManager.instance.Save();
        }
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
        if (PlayerManager.instance == null) return;

        foreach (var item in itemsToRemove)
        {
            PlayerManager.instance.items.Remove(item);
        }
        UiController.instance.RefreshInventory();
    }
}
    