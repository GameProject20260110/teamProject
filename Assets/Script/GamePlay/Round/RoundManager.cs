using UnityEngine;
using UnityEngine.UI;
public class RoundManager : MonoBehaviour
{
    public static RoundManager instance;
    [Header("라운드 설정")]
    public int currentRound = 1;
    public int targetScore = 0;

    public StageDataSo currentStageData;
    public Image enemyImage;
    private bool UseTestMode => TestModeManager.instance != null && TestModeManager.instance.isTestModeActive;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if(PlayerManager.instance != null && PlayerManager.instance.currentRound > 1)
        {
            currentRound = PlayerManager.instance.currentRound;
        }
        if (TestModeManager.instance != null && TestModeManager.instance.isTestModeActive)
        {
            TestModeManager.instance.ApplyTestStats();
        }
        StartRound();
    }

    public void StartRound()
    {
        if(UseTestMode)
        {
            targetScore = 9999;

            GimmickManager.instance?.ClearGimmick();
            var tm = TestModeManager.instance;
            if (!tm.noGimmick && tm.testGimmick != null)
            {
                GimmickManager.instance.currentActiveGimmick.Clear();
                foreach(var gimmick in tm.testGimmick)
                {
                    if (gimmick == null) return;
                    GimmickManager.instance.currentActiveGimmick.Add(gimmick);
                    gimmick.ExecuteGimmick();
                    Debug.Log("테스트 모드 기믹 강제 활성화");
                }
                UiController.instance?.RefreshGimmickIcons(GimmickManager.instance.currentActiveGimmick);
            }
            else if(tm.noGimmick)
            {
                Debug.Log("테스트 모드 기믹 비활성화");
            }

            if(GameManager.instance != null)
            {
                GameManager.instance.maxRerollCount = 9999;
            }
        }
        else
        {
            if(currentStageData != null)
            {
                RoundData roundData = currentStageData.GetRoundData(currentRound);

                if(roundData != null)
                {
                    targetScore = roundData.targetScore;

                    if(enemyImage != null)
                    {
                        enemyImage.sprite = roundData.enemyImage;
                    }

                    if(roundData.hasGimmick)
                    {
                        GimmickManager.instance.ApplyGimmick(currentRound);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"{currentRound}라운드 정보가 없습니다.");
            }
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.InitializeRoundData();
        }

        if(UiController.instance != null)
        {
            UiController.instance.HideAllPanels();
            UiController.instance.SetRollBtnInteractable(true);
            UiController.instance.SetConfirmBtnInteratable(false);
            UiController.instance.SetRollButtonToRoll();
        }

        if(GameManager.instance != null && GameManager.instance.diceManager != null)
        {
            GameManager.instance.diceManager.SetupDiceBoard();
        }
    }

    public void CompleteRound(int finalScore)
    {
        if (UiController.instance != null) UiController.instance.SetRollBtnInteractable(false);

        bool isSuccess = finalScore >= targetScore;

        
        if(isSuccess)
        {
            if(currentStageData != null && GameManager.instance != null)
            {
                int reward = currentStageData.GetGoldReward(currentRound);
                GameManager.instance.AddGold(reward);
                Debug.Log($"라운드 성공! 골드 {reward} 획득");
            }
            UiController.instance.ShowResultPanel(true, targetScore, finalScore, GameManager.instance.CurrentHearts);
        }
        else
        {
            GameManager.instance.ModifyHearts(-1);

            if(GameManager.instance.CurrentHearts > 0)
            {
                if (currentStageData != null && GameManager.instance != null)
                {
                    int reward = currentStageData.GetGoldReward(currentRound);
                    GameManager.instance.AddGold(reward);
                    Debug.Log($"라운드 실패 골드 {reward} 획득");
                }
                UiController.instance.ShowResultPanel(false, targetScore, finalScore, GameManager.instance.CurrentHearts);
            }
            else
            {
                GameManager.instance.HandleGameOver();
            }
        }
        GimmickManager.instance.ClearGimmick();
    }

    public void GoNextRound()
    {
        currentRound++;
        
        if(PlayerManager.instance.currentRound < currentRound)
        {
            PlayerManager.instance.currentRound = currentRound;
            PlayerManager.instance.Save();
        }
        Debug.Log($"{currentRound}라운드로 진입");
        StartRound();
    }
}


