using UnityEngine;
using UnityEngine.UI;
public class RoundManager : MonoBehaviour
{
    public static RoundManager instance;
    [Header("라운드 설정")]
    public int currentRound = 1;

    [SerializeField] private RoundController roundEffect;
    public StageDataSo currentStageData;
    public Image enemyImage;
    public EnemyData enemyData;
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
        if (UseTestMode)
        {
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

            if(PlayerManager.instance != null)
            {
                 PlayerManager.instance.gameRerollCount = 9999;
            }
        }
        else
        {
            roundEffect.PlayIntroAnim(currentRound);
            if (enemyImage != null)
            {
                enemyImage.sprite = BattleDataManager.instance?.GetEnemyImage();
            }

            // 보스전만 기믹
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
            UiController.instance.ResetItemCards();
        }

        if(VisualManager.instance != null)
            VisualManager.instance.ResetDiceColors(GameManager.instance.diceManager.GetAllDice());


        if (DeckManager.instance != null)
        {
            DeckManager.instance.InitializeDeck();
            DeckManager.instance.DrawDice();       
        }

        if(EnemyDeckManager.instance != null)
        {
            EnemyDeckManager.instance.InitializeDeck();
            EnemyDeckManager.instance.DrawEnemyDice();
        }

        if (BattleManager.instance != null)
        {
            BattleManager.instance.InitializeBattle();
        }
    }

    public void CompleteRound(bool isSuccess)
    {
        if (UiController.instance != null) 
            UiController.instance.SetRollBtnInteractable(false);

        if(PlayerManager.instance != null)
        {
            foreach(var item in PlayerManager.instance.items)
            {
                if (item == null) continue;
                item.RoundEnd();
            }
        }
        if(PlayerManager.instance.tempExtraSlotsCount > 0)
        {
            bool[] slots = PlayerManager.instance.SpecialSlots;
            int remove = 0;
            for(int i = slots.Length - 1; i >= 0 && remove < PlayerManager.instance.tempExtraSlotsCount; i--)
            {
                if (slots[i])
                {
                    slots[i] = false;
                    remove++;
                }
            }
            PlayerManager.instance.tempExtraSlotsCount = 0;
        }

        int currentHP = PlayerManager.instance != null ? PlayerManager.instance.heart : 0;

        // 클리어 시 선택 보상

        if(isSuccess)
        {
            int reward = BattleDataManager.instance?.GetGoldReward() ?? 10;
            GameManager.instance.AddGold(reward);

            // 보스전 클리어 후 맵 데이터 초기화
            if(BattleDataManager.instance?.isBossBattle == true)
            {
                MapManager.instance?.ClearMapSave();
                BattleDataManager.instance?.Clear();
            }
            UiController.instance.ShowResultPanel(true, currentHP);
        }
        else
        {
            GameManager.instance.HandleGameOver();
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
    }
}


