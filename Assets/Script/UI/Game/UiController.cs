using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{

    public static UiController instance = null;

    public DiceSkin defaultDiceSkin;

    [Header("인게임 정보 UI (상시 표시)")]
    public TextMeshProUGUI roundInfoText;  
    public TextMeshProUGUI targetScoreInfoText;
    public TextMeshProUGUI myScoreInfoText;
    public TextMeshProUGUI goldText;

    [Header("아이템 인벤토리")]
    public List<Image> itemIcon;

    [Header("라운드 결과 패널 (승리/패배)")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultTitleText;  
    public TextMeshProUGUI resultTargetScoreText;
    public TextMeshProUGUI resultMyScoreText;
    public Transform resultLifeContainer;
    public Image resultHeartPrefab;

    [Header("게임 오버 패널")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI goRoundText; 
    public TextMeshProUGUI goBestScoreText;
    public Image[] lastDice;

    [Header("다시 던지기")]
    public Button rollBtn;
    public TextMeshProUGUI rerollText;

    [Header("확정 버튼")]
    public Button confirmBtn;

    [Header("라이프")]
    public Transform lifeContainer;
    public Image heartPrefab;

    public GameObject settingPanel;

    private List<Image> hearts = new List<Image>();
    private List<Image> resultHearts = new List<Image>();

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

        SetUpItemSlotEvents();
        RefreshInventory();
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
        
        while (hearts.Count < lives)
        {
            Image newHeart = Instantiate(heartPrefab, lifeContainer);

            hearts.Add(newHeart);
        }

        for(int i = 0; i < hearts.Count; i++)
        {
            if(i < lives)
            {
                hearts[i].gameObject.SetActive(true);
            }
            else
            {
                hearts[i].gameObject.SetActive(false);
            }
        }
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

    public void RefreshInventory()
    {
        if (itemIcon == null) return;

        List<ItemSo> items = GetCurrentItems() ?? new List<ItemSo>();
        
        for (int i = 0; i < itemIcon.Count; i++)
        {
            if (itemIcon[i] == null) return;
            if(i < items.Count && items[i] != null)
            {
                itemIcon[i].gameObject.SetActive(true);
                itemIcon[i].sprite = items[i].itemIcon;
                itemIcon[i].color = Color.white;
            }
            else
            {
                itemIcon[i].gameObject.SetActive(false);
            }
        }
    }

    public void HideAllPanels()
    {
        if (resultPanel) resultPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    public void ShowResultPanel(bool isSuccess, int targetScore, int currentScore, int currentLife)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        resultTitleText.text = isSuccess ? "Round Clear!" : "Round Failed";

        if (resultTargetScoreText) 
        {
            resultTargetScoreText.text = $"Target Score: {targetScore}";
        }

        if (resultMyScoreText) 
        {
            resultMyScoreText.text = $"My Score: {currentScore}";
        }

        UpdateResultHearts(currentLife);
        RefreshInventory();
    }

    private void UpdateResultHearts(int currentLife)
    {
        if (resultLifeContainer == null || resultHeartPrefab == null) return;

        while(resultHearts.Count < currentLife)
        {
            resultHearts.Add(Instantiate(resultHeartPrefab, resultLifeContainer));
        }

        for(int i = 0; i < resultHearts.Count; i++) 
        {
            resultHearts[i].gameObject.SetActive(i < currentLife);
        }
    }

    public void ShowGameOverPanel(int round, int bestScore, List<DiceData> datas, List<int> values)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (goRoundText)
        {
            goRoundText.text = $"You reached Round: {round}";
        }

        if (goBestScoreText)
        {
            goBestScoreText.text = $"Your Best Score: {bestScore}";
        }

        if(lastDice != null)
        {
            for (int i = 0; i < lastDice.Length; i++)
            {
                if(i < values.Count)
                {
                    lastDice[i].gameObject.SetActive(true);

                    DiceData data = null;
                    
                    if(datas != null && i < datas.Count)
                    {
                        data = datas[i];
                    }
                    int index = values[i];

                    if(data != null && data.skin != null)
                    {
                        lastDice[i].sprite = data.skin.GetSprite(index);
                    }
                    else if(defaultDiceSkin != null)
                    {
                        lastDice[i].sprite = defaultDiceSkin.GetSprite(index);
                    }
                }
                else
                {
                    lastDice[i].gameObject.SetActive(false);
                }
            }
        }
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

    private void SetUpItemSlotEvents()
    {
        if (itemIcon == null) return;

        for (int i = 0; i < itemIcon.Count; i++)
        {
            if (itemIcon[i] == null) continue;

            int index = i;
            var trigger = itemIcon[i].gameObject.GetComponent<EventTrigger>() ?? itemIcon[i].gameObject.AddComponent<EventTrigger>();

            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(_ => OnItemSlotHover(index));
            trigger.triggers.Add(enterEntry);

            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => OnItemSlotExit(index));
            trigger.triggers.Add(exitEntry);
        }
    }

    private void OnItemSlotHover(int index)
    {
        List<ItemSo> items = GetCurrentItems();
        if (items == null || index >= items.Count || items[index] == null) return;

        PopupManager.instance?.OpenPopup(items[index], itemIcon[index].GetComponent<RectTransform>());
    }
    private void OnItemSlotExit(int index)
    {
        PopupManager.instance?.ClosePopup();
    }

    private List<ItemSo> GetCurrentItems()
    {
        if(TestModeManager.instance != null && TestModeManager.instance.isTestModeActive)
        {
            return TestModeManager.instance.testItem;
        }
        return PlayerManager.instance?.items;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingPanel();
        }
    }

    public void ToggleSettingPanel()
    {
        if (settingPanel == null) return;
        settingPanel.SetActive(!settingPanel.activeSelf);
    }
}
