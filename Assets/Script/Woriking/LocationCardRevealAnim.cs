using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LocationCardRevealAnimator : MonoBehaviour
{
    [Serializable]
    public class CardRevealSetting
    {
        public RectTransform card;
        public Vector2 startOffset = new Vector2(700f, 500f);
        public Vector2 arcOffset = new Vector2(0f, 150f);

        [Range(0.05f, 1f)]
        public float startScale = 0.3f;

        [Range(1, 3)]
        public int spinCount = 1;

        public float delay = 0f;
    }

    [Header("카드별 설정 (왼쪽부터 순서대로 등록)")]
    public CardRevealSetting[] cards;

    [Header("공통 타이밍")]
    public float duration = 1f;

    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve scaleCurve = CreateOvershootCurve();
    public AnimationCurve spinEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public event Action OnRevealComplete;
    private CancellationTokenSource _cts;

    private static AnimationCurve CreateOvershootCurve()
    {
        var curve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.7f, 1.08f),
            new Keyframe(1f, 1f)
        );
        for (int i = 0; i < curve.length; i++)
        {
            curve.SmoothTangents(i, 0f);
        }
        return curve;
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public void PlayReveal()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        PlayRevealAsync(_cts.Token).Forget();
    }

    

    private async UniTaskVoid PlayRevealAsync(CancellationToken token)
    {
        var tasks = new UniTask[cards.Length];
        for (int i = 0; i < cards.Length; i++)
        {
            var setting = cards[i];
            tasks[i] = setting != null && setting.card != null
                ? RevealRoutine(setting, token)
                : UniTask.CompletedTask;
        }

        try
        {
            await UniTask.WhenAll(tasks);
            OnRevealComplete?.Invoke();
        }
        catch (OperationCanceledException)
        {
            // 오브젝트 파괴 등으로 취소된 경우 조용히 무시
        }
    }

    private async UniTask RevealRoutine(CardRevealSetting c, CancellationToken token)
    {
        if (c.delay > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(c.delay), cancellationToken: token);

        RectTransform rt = c.card;

        Vector2 targetPos = rt.anchoredPosition;
        Vector2 startPos = targetPos + c.startOffset;
        Vector2 controlPos = Vector2.Lerp(startPos, targetPos, 0.5f) + c.arcOffset;

        // 시작 상태로 즉시 세팅
        rt.anchoredPosition = startPos;
        rt.localScale = new Vector3(c.startScale, c.startScale, 1f);
        rt.localRotation = Quaternion.identity;

        float t = 0f;
        while (t < duration)
        {
            token.ThrowIfCancellationRequested();

            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / duration);

            // 1) 베지어 곡선 이동
            float moveT = moveCurve.Evaluate(n);
            rt.anchoredPosition = QuadraticBezier(startPos, controlPos, targetPos, moveT);

            // 2) 크기 (오버슈트 포함)
            float sizeT = scaleCurve.Evaluate(n);
            float overallScale = Mathf.LerpUnclamped(c.startScale, 1f, sizeT);

            // 3) 스핀 (Y축 회전을 흉내내는 X 스케일 오실레이션)
            float spinProgress = spinEase.Evaluate(n);
            float spinAngleRad = spinProgress * c.spinCount * 2f * Mathf.PI;
            float flipFactor = Mathf.Cos(spinAngleRad); // 정수 바퀴면 n=1일 때 항상 1

            rt.localScale = new Vector3(overallScale * flipFactor, overallScale, 1f);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        // 오차 없이 최종값으로 스냅
        rt.anchoredPosition = targetPos;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }

    private static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        float u = 1f - t;
        return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
    }
    
    [ContextMenu("테스트 재생")]
    private void TestPlayReveal() => PlayReveal();
}