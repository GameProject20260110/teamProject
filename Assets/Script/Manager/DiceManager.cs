using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DiceManager : MonoBehaviour
{
  
    [Header("UI 연결")]
    public RectTransform rollArea;

    [Header("주사위 오브젝트")]
    public Dice[] panelDiceScript;

    [Header("기본 설정")]
    public DiceData defaultDice;
   
    public bool isRolling => _isRolling;

    private bool UseTestMode => TestModeManager.instance != null && TestModeManager.instance.isTestModeActive;

    private bool _isRolling = false;
    private float padding = 100.0f;

    void Start()
    {
        if(GameManager.instance != null) GameManager.instance.diceManager = this;

        SetupDiceBoard();
    }

    public void SetupDiceBoard()
    {
        if (panelDiceScript == null) return;

        DiceData fallbackDice = defaultDice;

        if(PlayerManager.instance != null && PlayerManager.instance.defaultDice != null)
        {
            fallbackDice = PlayerManager.instance.defaultDice;
        }

        bool isSlotUnlock = false;

        for(int i = 0; i < panelDiceScript.Length; i++)
        {
            if (panelDiceScript[i] == null) continue;

            if (UseTestMode)
            {
                isSlotUnlock = TestModeManager.instance.testDiceSlot[i];
            }
            else
            {
                if(PlayerManager.instance != null && i < PlayerManager.instance.SpecialSlots.Length)
                {
                    isSlotUnlock = PlayerManager.instance.SpecialSlots[i];
                }
            }

            if (!isSlotUnlock)
            {
                panelDiceScript[i].transform.parent.gameObject.SetActive(false);
                continue;
            }

            DiceData dataToUse = null;

            if (UseTestMode)
            {
                if (TestModeManager.instance.testAbilities != null && i < TestModeManager.instance.testAbilities.Length && TestModeManager.instance.testAbilities[i] != null)
                {
                    dataToUse = TestModeManager.instance.testAbilities[i];
                }
                else
                {
                    dataToUse = fallbackDice;
                }
            }
            else
            {
                if (PlayerManager.instance != null && PlayerManager.instance.dices != null && i < PlayerManager.instance.dices.Count)
                {
                    dataToUse = PlayerManager.instance.dices[i];
                }
                else
                {
                    dataToUse = fallbackDice;
                }
            }

            if(dataToUse != null)
            {
                panelDiceScript[i].transform.parent.gameObject.SetActive(true);
                panelDiceScript[i].gameObject.SetActive(true);
                panelDiceScript[i].Initialize(i, dataToUse);
            }
        }
    }

    public void StartRolling()
    {
        if (_isRolling) return;
        StartCoroutine(RollRoutine());
    }

    IEnumerator RollRoutine()
    {
        _isRolling = true;

        UiController.instance.SetRollBtnInteractable(false);

        float rollDuration = 1.5f;

        DG.Tweening.Sequence rollSequence = DOTween.Sequence();

        for(int i = 0; i < panelDiceScript.Length; i++)
        {
            if (panelDiceScript[i] == null || !panelDiceScript[i].gameObject.activeSelf) continue;

            Dice currentDice = panelDiceScript[i];
            Transform diceTransform = currentDice.transform;

            float currentDuration = rollDuration + Random.Range(-0.2f, 0.2f);

            currentDice.StartRoll(rollDuration * 0.9f);

            // 눈금 값 결정
            int randomResult = Random.Range(1, 7);

            // 테스트 모드(눈금 강제)
            if (UseTestMode && TestModeManager.instance.testValues != null && i < TestModeManager.instance.testValues.Length)
            {
                if (TestModeManager.instance.testValues[i] > 0)
                {
                    randomResult = Mathf.Clamp(TestModeManager.instance.testValues[i], 1, 6);
                }
            }

            int boundCount = Random.Range(2, 5);

            float stopTime = currentDuration * 0.3f;
            float boundTime = (currentDuration - stopTime) / boundCount;
            
            Vector3 StopPoint = GetRandomPointInRollArea();

            // 이동
            DG.Tweening.Sequence moveSeq = DOTween.Sequence();

            for(int j = 0; j < boundCount; j++)
            {
                Vector3 randomPoint = GetRandomPointInRollArea();
                moveSeq.Append(diceTransform.DOMove(randomPoint, boundTime).SetEase(Ease.InOutQuad));
            }

            moveSeq.Append(diceTransform.DOMove(StopPoint, stopTime).SetEase(Ease.OutCubic).OnComplete(()=>
            {
                currentDice.SetResult(randomResult);
            }));

            // 주사위 회전
            Tween roateTween = diceTransform
                .DORotate(new Vector3(0, 0, 365 * 5), rollDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic);

            rollSequence.Join(moveSeq);
            rollSequence.Join(roateTween);
        }

        yield return rollSequence.WaitForCompletion();

        yield return new WaitForSeconds(1.0f);

        // 주사위가 다시 제자리로
        DG.Tweening.Sequence returnSequence = DOTween.Sequence();

        for(int i = 0; i < panelDiceScript.Length; i++)
        {
            if (panelDiceScript[i] == null || !panelDiceScript[i].transform.parent.gameObject.activeSelf) continue;
            Transform diceTransform = panelDiceScript[i].transform;

            returnSequence.Join(diceTransform.DOLocalMove(Vector3.zero, 0.5f));
            returnSequence.Join(diceTransform.DOLocalRotate(Vector3.zero, 0.5f));
        }

        yield return returnSequence.WaitForCompletion();


        // 점수 계산
        var result = ScoreManager.instance.CalculateScore(panelDiceScript, ScoreManager.DiceType.Roll);
        int finalScore = result.finalScore;
        List<ScoreEventData> events = result.events;
        if(ScoreVisualizer.instance != null)
        {
            yield return StartCoroutine(ScoreVisualizer.instance.PlayScoreEventSequence(panelDiceScript, events));
        }
        if(GameManager.instance != null)
        {
            GameManager.instance.ProcessRollResult(result.finalScore, result.consumedItems);
        }

        _isRolling = false;
    }
    public Sprite[] GetLastDiceSprites()
    {
        Sprite[] lastDiceSprite = new Sprite[panelDiceScript.Length];
        for(int i = 0; i < panelDiceScript.Length; i++)
        {
            if (panelDiceScript[i] != null && panelDiceScript[i].gameObject.activeSelf) lastDiceSprite[i] = panelDiceScript[i].GetCurrentSprite();
        }
        return lastDiceSprite;
    }

    // 화면 랜덤 좌표 
    Vector3 GetRandomPointInRollArea()
    {
        if(rollArea == null)
        {
            return transform.position;
        }

        Rect rect = rollArea.rect;

        float safePadX = Mathf.Min(padding, rect.width * 0.3f);
        float safePadY = Mathf.Min(padding, rect.height * 0.3f);

        float localX = Random.Range(rect.xMin + safePadX, rect.xMax - safePadX);
        float localY = Random.Range(rect.yMin + safePadY, rect.yMax - safePadY);
        return rollArea.TransformPoint(localX, localY, 0);
    }
}
