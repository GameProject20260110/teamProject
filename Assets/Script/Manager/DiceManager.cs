using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Security.Cryptography;
using System.Drawing;

public class DiceManager : MonoBehaviour
{
    [Header("테스트용")]
    public bool isTestMode = false;
    public DiceData[] testAbilities;
    public int[] testDiceValues = new int[6];
    
    [Header("UI 연결")]
    public RectTransform rollArea;

    [Header("주사위 오브젝트")]
    public Dice[] panelDiceScript;

    [Header("주사위 데이터")]
    //public List<DiceData> diceDatas;

    [Header("기본 설정")]
   
    public bool isRolling => _isRolling;

    private bool _isRolling = false;
    // 원래 자리 기억용
    private Vector3[] _originalPosition;
    private float padding = 100.0f;

    void Start()
    {
        if(GameManager.instance != null)
        {
            GameManager.instance.diceManager = this;
        }

        SetupDiceBoard();

        _originalPosition = new Vector3[panelDiceScript.Length];
        for(int i = 0; i < panelDiceScript.Length; i++)
        {
            _originalPosition[i] = panelDiceScript[i].transform.position;
        }
    }

    public void SetupDiceBoard()
    {
        for(int i = 0; i < panelDiceScript.Length; i++)
        {
            DiceData dataToUse = PlayerManager.instance.defaultDice;
            //if(i < diceDatas.Count && diceDatas[i] != null)
            //{
                
                
            //}

            if(isTestMode && testAbilities != null && i < testAbilities.Length)
            {
                if (testAbilities[i] != null)
                {
                    dataToUse = testAbilities[i];
                }
            }

            // 2. 테스트 데이터가 없거나 테스트 모드가 아니면 -> 기본 덱 사용
            if (dataToUse == null)
            {
                if (PlayerManager.instance != null && i < PlayerManager.instance.dices.Count)
                {
                    dataToUse = PlayerManager.instance.dices[i];
                }
                // 그래도 없으면 디폴트 사용
                else
                {
                    dataToUse = PlayerManager.instance.defaultDice;
                }
            }

            if (dataToUse != null)
            {
                panelDiceScript[i].Initialize(i, dataToUse);
            }
            else
            {
                Debug.LogError($"[DiceManager] {i}번 주사위 데이터가 없습니다!");
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
            if (panelDiceScript[i] == null) continue;

            Dice currentDice = panelDiceScript[i];
            Transform diceTransform = currentDice.transform;

            float currentDuration = rollDuration + Random.Range(-0.2f, 0.2f);

            currentDice.StartRoll(rollDuration * 0.9f);

            // 눈금 값 결정
            int randomResult = Random.Range(1, 7);

            // 테스트 모드(눈금 강제)
            if(isTestMode && testDiceValues != null && i < testDiceValues.Length)
            {
                if (testDiceValues[i] > 0)
                {
                    randomResult = Mathf.Clamp(testDiceValues[i], 1, 6);
                }
            }


            // dice 객체에 데이터 입력
            currentDice.SetResult(randomResult);

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
            if (panelDiceScript[i] == null) continue;
            Transform diceTransform = panelDiceScript[i].transform;

            returnSequence.Join(diceTransform.DOMove(_originalPosition[i], 0.5f));
            returnSequence.Join(diceTransform.DORotate(Vector3.zero, 0.5f));
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
            GameManager.instance.ProcessRollResult(finalScore);
        }

        _isRolling = false;
    }
    public Sprite[] GetLastDiceSprites()
    {
        Sprite[] lastDiceSprite = new Sprite[panelDiceScript.Length];
        for(int i = 0; i < panelDiceScript.Length; i++)
        {
            lastDiceSprite[i] = panelDiceScript[i].GetCurrentSprite();
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
