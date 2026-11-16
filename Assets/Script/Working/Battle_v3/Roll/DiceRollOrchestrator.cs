using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class DiceRollOrchestrator
{
    private HandData _hand;
    private readonly DiceRollController _diceRollController;
    private readonly HandLayoutView _handLayoutView;
    private readonly DiceRollPanelView _panelView;

    private CancellationTokenSource _skipCts;

    public DiceRollOrchestrator(HandData hand, DiceRollController diceRollController, HandLayoutView handLayoutView, DiceRollPanelView panelView)
    {
        _hand = hand;
        _diceRollController = diceRollController;
        _handLayoutView = handLayoutView;
        _panelView = panelView;
    }

    public async UniTask RunAsync(CancellationToken ct)
    {
        if (_hand == null || _diceRollController == null) return;
        if (_hand.Count == 0) return;

        bool prepared = _panelView != null && _panelView.Prepare(_hand.Cards);
        _diceRollController.RollHand();

        if(!prepared)
        {
            RefreshHandPower();
            return;
        }

        _skipCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        CancellationToken skipToken = _skipCts.Token;
        _panelView.OnSkipRequested += HandleSkipRequested;
        _panelView.SetSkipEnabled(true);

        try
        {
            try
            {
                await _panelView.PlayOpenAsync(skipToken);
                await _panelView.PlayRollAsync(skipToken);
            }
            catch(OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                _panelView.ShowResultInstant();
            }
            finally
            {
                _panelView.OnSkipRequested -= HandleSkipRequested;
                _panelView.SetSkipEnabled(false);
                _skipCts.Dispose();
                _skipCts = null;
            }

            RefreshHandPower();
            await _panelView.WaitResultHoldAsync(ct);
            await _panelView.CloseAsync(ct);
        }
        finally
        {
            if (ct.IsCancellationRequested) _panelView.HideImmediate();
        }
    }

    private void HandleSkipRequested()
    {
        _skipCts?.Cancel();
    }

    private void RefreshHandPower()
    {
        if (_handLayoutView != null) 
        {
            _handLayoutView.RefreshAllPower();
        }
    }
}
