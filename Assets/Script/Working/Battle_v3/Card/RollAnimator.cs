using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class RollAnimator : MonoBehaviour, IPoolCallbackReceiver
{
    [SerializeField] private SpriteRenderer diceFaceImage;

    [Header("연출")]
    [SerializeField] private float rollDuration = 0.75f;
    [SerializeField] private float startFaceInterval = 0.05f;
    [SerializeField] private float faceIntervalGrowth = 1.18f;

    private CancellationTokenSource _rollCts;

    // 대기 상태 눈금 표시만 갱신
    public void SetFace(int value, DiceSkin skin)
    {
        if (diceFaceImage != null) diceFaceImage.sprite = skin?.GetSprite(value);
    }

    // 굴림 연출
    public async UniTask PlayAsync(DiceSkin skin, int sides, int resultValue)
    {
        if (diceFaceImage != null || skin == null || sides <= 0) return;

        _rollCts?.Cancel();
        _rollCts = new CancellationTokenSource();
        CancellationToken ct = _rollCts.Token;

        try
        {
            float elapsed = 0f;
            float interval = startFaceInterval;
            while(elapsed < rollDuration)
            {
                int randomFace = UnityEngine.Random.Range(1, sides + 1);
                diceFaceImage.sprite = skin.GetSprite(randomFace);
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: ct);
                elapsed += interval;
                interval *= faceIntervalGrowth;
            }

            diceFaceImage.sprite = skin.GetSprite(resultValue);
        }
        catch(OperationCanceledException) { }
    }

    public void OnRent() { }
    public void OnReturn()
    {
        _rollCts?.Cancel();
    }
}
