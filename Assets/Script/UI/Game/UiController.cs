using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    public static UiController instance = null;

    [Header("모듈")]
    public ItemInventoryUI inventoryUI;
    public LifeUI lifeUI;
    public ResultPanelUI resultUI;
    public GameOverPanelUI gameOverUI;

    [Header("인게임 정보 UI (상시 표시)")]
    public TextMeshProUGUI roundInfoText;  
    public TextMeshProUGUI targetScoreInfoText;
    public TextMeshProUGUI myScoreInfoText;
    public TextMeshProUGUI goldText;

    [Header("버튼")]
    public Button rollBtn;
    public TextMeshProUGUI rerollText;
    public Image rollBtnImage;
    public Sprite rollSprite;
    public Sprite rerollSprite;
    public Button confirmBtn;

    public GameObject settingPanel;

    private void Awake()
    {
        if (instance == null)
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
        //AudioManager.instance.PlayBgm(AudioManager.Bgm.Battle, true);
        if(settingPanel != null) settingPanel.SetActive(false);

        if(GameManager.instance != null)
        {
            SubscribeToEvents();
        }
        RefreshInventory();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingPanel();
        }
    }

    private void OnDisable()
    {
        if (GameManager.instance != null) UnSubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        GameManager.instance.OnGoldChanged += UpdateGoldUi;
        GameManager.instance.OnScoreChanged += UpdateScoreUi;
        GameManager.instance.OnHeartsChanged += UpdateLivesUi;
        GameManager.instance.OnRoundAndGoalChanged += UpdateRoundAndGoalUi;
        GameManager.instance.OnRerollCountChanged += UpdateRerollUi;
    }

    private void UnSubscribeToEvents()
    {
        GameManager.instance.OnGoldChanged -= UpdateGoldUi;
        GameManager.instance.OnScoreChanged -= UpdateScoreUi;
        GameManager.instance.OnHeartsChanged -= UpdateLivesUi;
        GameManager.instance.OnRoundAndGoalChanged -= UpdateRoundAndGoalUi;
        GameManager.instance.OnRerollCountChanged -= UpdateRerollUi;
    }

    private void UpdateGoldUi(int gold)
    {
        if(goldText != null)
        {
            goldText.text = gold.ToString("N0");
        }
    }

    private void UpdateScoreUi(int score)
    {
        if(myScoreInfoText != null)
        {
            myScoreInfoText.SetText("{0}", score);
        }
    }

    private void UpdateLivesUi(int lives)
    {
        lifeUI?.UpdateHearts(lives);
    }

    private void UpdateRoundAndGoalUi(int round, int targetScore)
    { 
        Debug.Log($"[UI] 라운드 갱신 시도: Round {round}, Target {targetScore}");
        if (roundInfoText != null)
        {
            roundInfoText.SetText("{0}", round);
        }

        if (targetScoreInfoText != null)
        {
            targetScoreInfoText.SetText("target score : {0}", targetScore);
        }
    }

    public void SetRollButtonToRoll()
    {
        if(rollBtnImage != null && rollSprite != null)
        {
            rollBtnImage.sprite = rollSprite;
        }
    }

    public void SetRollButtnonToReroll()
    {
        if(rollBtnImage != null && rerollSprite != null)
        {
            rollBtnImage.sprite = rerollSprite;
        }
    }

    public void RefreshInventory()
    {
        inventoryUI?.Refresh();
    }

    public void HideAllPanels()
    {
        resultUI?.Hide();
        gameOverUI?.Hide();

    }

    public void ShowResultPanel(bool isSuccess, int targetScore, int currentScore, int currentLife)
    {
        resultUI?.Show(isSuccess, targetScore, currentScore, currentLife);
        RefreshInventory();
    }

    public void ShowGameOverPanel(int round, int bestScore, List<DiceData> diceDatas, List<int> values)
    {
        gameOverUI?.Show(round, bestScore, diceDatas, values);
    }

    public void UpdateRerollUi(int count)
    {
        if(rerollText != null)
        {
            rerollText.SetText("Reroll: {0}", count);
        }
    }

    public void SetRollBtnInteractable(bool state)
    {
        if(rollBtn != null)
        {
            rollBtn.interactable = state;
        }
    }

    public void SetConfirmBtnInteratable(bool state)
    {
        if (confirmBtn != null)
        {
            confirmBtn.interactable = state;
        }
    }

    public void GotoLobby()
    {
        GameManager.instance.LoadHomeScreen();
    }

    public void ToggleSettingPanel()
    {
        if (settingPanel == null) return;
        settingPanel.SetActive(!settingPanel.activeSelf);
    }
}
