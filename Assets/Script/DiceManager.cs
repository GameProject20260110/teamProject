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

    [Header("UI 연결")]
    public RectTransform rollArea;

    [Header("주사위 오브젝트")]
    public Dice[] panelDiceScript;

    [Header("주사위 데이터")]
    public List<DiceData> diceDatas;

    [Header("플레이어 정보")]
    public PlayerSo player;

    [Header("기본 설정")]
    public DiceData defaultDiceData;
   
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
        
        SetupUseDiceScript();

        SetupDiceBoard();

        _originalPosition = new Vector3[panelDiceScript.Length];
        for(int i = 0; i < panelDiceScript.Length; i++)
        {
            if(panelDiceScript[i] != null)
                _originalPosition[i] = panelDiceScript[i].transform.position;
        }
    }

    public void SetupUseDiceScript()
    {
        for(int i = 0;i< panelDiceScript.Length; i++)
        {
            if (!panelDiceScript[i].gameObject.activeSelf)
            {
                panelDiceScript[i] = null;
            }
        }
    }

    public void SetupDiceBoard()
    {
        for(int i = 0; i < panelDiceScript.Length; i++)
        {
            DiceData dataToUse = defaultDiceData;

            //if(i < diceDatas.Count && diceDatas[i] != null)
            //{
            //    dataToUse = diceDatas[i];
            //}
            
            dataToUse = player.DiceSo[i];
            if(panelDiceScript [i] != null)
                panelDiceScript[i].Initialize(i, dataToUse);
            
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
        
      
        int finalScore = ScoreManager.instance.CalculateScore(panelDiceScript);

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

    public void ResetForNewRound()
    {
        _isRolling = false;
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
