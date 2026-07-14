using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class DiceRoller : MonoBehaviour
{
    public bool isRolling { get; private set; } = false;

    [Header("굴림 영역 (뷰포트 비율 0~0.5)")]
    [SerializeField] private float marginX = 0.1f;
    [SerializeField] private float marginY = 0.1f;
    [SerializeField] private float boardDepth = 10f; // 카메라 - 보드 평면 거리

    private AudioManager _audioManager;
    private UiController _uiController;

    [Inject]
    public void Construct(AudioManager audioManager, UiController uiController)
    {
        _audioManager = audioManager;
        _uiController = uiController;
    }

    public async UniTask StartRoll(Dice[] allDice)
    {
        if (isRolling) return;
        await RollRoutine(allDice);
    }

    private async UniTask RollRoutine(Dice[] allDice)
    {
        isRolling = true;
        float rollDuration = 1.5f;
        _audioManager.PlaySfx("Roll");
        var rollSeq = BuildRollSequence(allDice, rollDuration);
        await rollSeq.AsyncWaitForCompletion();
        await UniTask.Delay(1000);
        var returnSeq = BuildReturnSequence(allDice);
        await returnSeq.AsyncWaitForCompletion();
        isRolling = false;
    }

    private DG.Tweening.Sequence BuildRollSequence(Dice[] allDice, float duration)
    {
        var rollSeq = DOTween.Sequence();
        for (int i = 0; i < allDice.Length; i++)
        {
            Transform diceTransform = allDice[i].transform;
            Dice currentDice = allDice[i];
            float currentDuration = duration + Random.Range(-0.2f, 0.2f);
            int index = GetResultValue(i);
            currentDice.StartRoll(duration * 0.9f);
            var moveSeq = BuildMoveSequence(currentDice, currentDuration, index);
            Tween rotate = diceTransform.DORotate(new Vector3(0, 0, 365 * 5), duration, RotateMode.FastBeyond360);
            rollSeq.Join(moveSeq);
            rollSeq.Join(rotate);
        }
        return rollSeq;
    }

    private DG.Tweening.Sequence BuildMoveSequence(Dice dice, float duration, int resultValue)
    {
        Transform diceTransform = dice.transform;
        int boundCount = Random.Range(3, 5);
        float stopTime = duration * 0.3f;
        float boundTime = (duration - stopTime) / boundCount;
        Vector3 stopPoint = GetRandomPointInRollArea();
        var moveSeq = DOTween.Sequence();
        for (int j = 0; j < boundCount; j++)
        {
            Vector3 randomPoint = GetRandomPointInRollArea();
            moveSeq.Append(diceTransform.DOMove(randomPoint, boundTime).SetEase(Ease.InOutQuad));
        }
        moveSeq.Append(diceTransform.DOMove(stopPoint, stopTime).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            dice.SetResult(resultValue);
        }));
        return moveSeq;
    }

    private DG.Tweening.Sequence BuildReturnSequence(Dice[] allDice)
    {
        var returnSeq = DOTween.Sequence();
        for (int i = 0; i < allDice.Length; i++)
        {
            if (allDice[i] == null || !allDice[i].gameObject.activeInHierarchy) continue;
            Transform diceTransform = allDice[i].transform;
            returnSeq.Join(diceTransform.DOLocalMove(Vector3.zero, 0.5f));
            returnSeq.Join(diceTransform.DOLocalRotate(Vector3.zero, 0.5f));
        }
        return returnSeq;
    }

    private int GetResultValue(int index) => Random.Range(1, 7);

    private Vector3 GetRandomPointInRollArea()
    {
        Camera cam = Camera.main;
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(marginX, marginY, boardDepth));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1f - marginX, 1f - marginY, boardDepth));
        float x = Random.Range(min.x, max.x);
        float y = Random.Range(min.y, max.y);
        return new Vector3(x, y, 0f);
    }
}