using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;

// 드로우 페이즈의 비동기 시퀀스 지휘 - 로직과 연출을 순서대로 실행
public class DrawOrchestrator
{
    private readonly DrawController _drawController;
    private readonly HandLayoutView _handLayoutView;
    private readonly HanddrawConfig _config;

    private readonly List<UniTask> _flights = new List<UniTask>();

    public DrawOrchestrator(DrawController drawController, HandLayoutView handLayoutView, HanddrawConfig config)
    {
        _drawController = drawController;
        _handLayoutView = handLayoutView;
        _config = config;
    }

    public async UniTask RunDrawSequence(bool isFirstTurn, CancellationToken ct)
    {
        if (_drawController == null || _handLayoutView == null) return;

        int requested;
        if (_config != null) requested = isFirstTurn ? _config.firstTurnDrawCount : _config.perTurnDrawCount;
        else requested = isFirstTurn ? 6 : 2;

        int drawCount = _drawController.GetDrawableCount(requested);
        if (drawCount <= 0) return;

        float interval = _config != null ? _config.drawInterval : 0.12f;

        _handLayoutView.BeginDrawBatch(drawCount);
        _flights.Clear();

        try
        {
            for(int i = 0; i< drawCount; i++)
            {
                ct.ThrowIfCancellationRequested();

                if(_drawController.NeedsReshuffle())
                {
                    await _handLayoutView.PlayReshuffleAsync(ct);
                    _drawController.ReshuffleDiscardIntoDeck();
                }

                CardRuntime card = _drawController.DrawOne();
                UnityEngine.Debug.Log($"[Draw] {i}번 출발 t={UnityEngine.Time.time:F2}"); // 임시 - 확인 후 삭제
                if (card == null) break;

                _flights.Add(_handLayoutView.PlayDrawCardAsync(card, ct));

                bool isLast = i == drawCount - 1;
                if(!isLast && interval > 0f) 
                    await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: ct);
            }
            await UniTask.WhenAll(_flights);
        }
        finally
        {
            _flights.Clear();
            _handLayoutView.EndDrawBatch();
        }
    } 
}
