using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class DiceRoller : MonoBehaviour
{
    public bool isRolling { get; private set; } = false;
    private float padding = 100.0f;

    private AudioManager _audioManager;
    private UiController _uiController;

    [Inject]
    public void Construct(AudioManager audioManager, UiController uiController)
    {
        _audioManager = audioManager;
        _uiController = uiController;
    }

    public async UniTask StartRoll(Dice[] allDice, RectTransform rollArea)
    {
        if (isRolling) return;
        await RollRoutine(allDice, rollArea);
    }

    private async UniTask RollRoutine(Dice[] allDice, RectTransform rollArea)
    {
        isRolling = true;
        _uiController.SetRollBtnInteractable(false);
        float rollDuration = 1.5f;
        _audioManager.PlaySfx("Roll");
        DG.Tweening.Sequence rollSeq = BuildRollSequence(allDice, rollArea, rollDuration);
        await rollSeq.AsyncWaitForCompletion();
        await UniTask.Delay(1000);
        DG.Tweening.Sequence returnSeq = BuildReturnSequence(allDice);
        await returnSeq.AsyncWaitForCompletion();
        isRolling = false;
    }

    private DG.Tweening.Sequence BuildRollSequence(Dice[] allDice, RectTransform rollArea, float duration)
    {
        DG.Tweening.Sequence rollSeq = DOTween.Sequence();
        for (int i = 0; i < allDice.Length; i++)
        {
            Transform diceTransform = allDice[i].transform;
            Dice currentDice = allDice[i];
            float currentDuration = duration + Random.Range(-0.2f, 0.2f);
            int index = GetResultValue(i);
            currentDice.StartRoll(duration * 0.9f);
            DG.Tweening.Sequence moveSeq = BuildMoveSequence(currentDice, rollArea, currentDuration, index);
            Tween rotate = diceTransform.DORotate(new Vector3(0, 0, 365 * 5), duration, RotateMode.FastBeyond360);
            rollSeq.Join(moveSeq);
            rollSeq.Join(rotate);
        }
        return rollSeq;
    }

    private DG.Tweening.Sequence BuildMoveSequence(Dice dice, RectTransform rollArea, float duration, int resultValue)
    {
        Transform diceTransform = dice.transform;
        int boundCount = Random.Range(3, 5);
        float stopTime = duration * 0.3f;
        float boundTime = (duration - stopTime) / boundCount;
        Vector3 stopPoint = GetRandomPointInRollArea(rollArea);
        DG.Tweening.Sequence moveSeq = DOTween.Sequence();
        for (int j = 0; j < boundCount; j++)
        {
            Vector3 randomPoint = GetRandomPointInRollArea(rollArea);
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
        DG.Tweening.Sequence returnSeq = DOTween.Sequence();
        for (int i = 0; i < allDice.Length; i++)
        {
            if (allDice[i] == null || !allDice[i].gameObject.activeInHierarchy) continue;
            Transform diceTransform = allDice[i].transform;
            returnSeq.Join(diceTransform.DOLocalMove(Vector3.zero, 0.5f));
            returnSeq.Join(diceTransform.DOLocalRotate(Vector3.zero, 0.5f));
        }
        return returnSeq;
    }

    private int GetResultValue(int index)
    {
        int result = Random.Range(1, 7);
        return result;
    }

    Vector3 GetRandomPointInRollArea(RectTransform rollArea)
    {
        if (rollArea == null) return transform.position;
        Rect rect = rollArea.rect;
        float safePadX = Mathf.Min(padding, rect.width * 0.3f);
        float safePadY = Mathf.Min(padding, rect.height * 0.3f);
        float localX = Random.Range(rect.xMin + safePadX, rect.xMax - safePadX);
        float localY = Random.Range(rect.yMin + safePadY, rect.yMax - safePadY);
        return rollArea.TransformPoint(localX, localY, 0);
    }
}
