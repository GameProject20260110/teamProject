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
        StartRound();
    }

    public void StartRound()
    {
        if(UseTestMode)
        {
            targetScore = 9999;
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
            GameManager.instance.NotifyAllUI();
        }

        if(UiController.instance != null)
        {
            UiController.instance.HideAllPanels();
            UiController.instance.SetRollBtnInteractable(true);
            UiController.instance.SetConfirmBtnInteratable(false);
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
            UiController.instance.ShowResultPanel(true, targetScore, finalScore, GameManager.instance.currentLives);
        }
        else
        {
            GameManager.instance.ModifyLives(-1);

            if(GameManager.instance.currentLives > 0)
            {
                UiController.instance.ShowResultPanel(false, targetScore, finalScore, GameManager.instance.currentLives);
            }
            else
            {
                GameManager.instance.HandleGameOver();
            }
        }
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


