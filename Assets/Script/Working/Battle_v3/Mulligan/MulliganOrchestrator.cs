using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

public class MulliganOrchestrator
{
    private readonly HandData _hand;
    private readonly MulliganController _mulliganController;
    private readonly HandLayoutView _handLayoutView;
    private readonly MulliganPanelView _panelView;

    private readonly List<int> _selectedIndices = new List<int>();
    private readonly List<CardRuntime> _newCards = new List<CardRuntime>();
    private UniTaskCompletionSource _confirmSource;

    public MulliganOrchestrator(HandData hand, MulliganController mulliganController, HandLayoutView handLayoutView, MulliganPanelView panelView)
    {
        _handLayoutView = handLayoutView;
        _mulliganController = mulliganController;
        _panelView = panelView;
        _hand = hand;
    }

    public async UniTask RunMulliganAsync(CancellationToken ct)
    {
        if (_hand == null || _mulliganController == null || _handLayoutView == null || _panelView == null) return;
        if (_hand.Count == 0) return;

        _mulliganController.Begin();
        _panelView.SetSelectableCount(_mulliganController.RemainingSelectable);
        _panelView.OnCardClicked += HandleCardClicked;
        _panelView.OnConfirmClicked += HandleConfirmClicked;

        try
        {
            bool shown = await _panelView.ShowAsync(_hand.Cards, ct);
            if (!shown) return;

            _confirmSource = new UniTaskCompletionSource();
            _panelView.SetInputEnabled(true);
            await _confirmSource.Task.AttachExternalCancellation(ct);
            _panelView.SetInputEnabled(false);

            _mulliganController.GetSelectedIndices(_selectedIndices);
            _mulliganController.ApplyReplacements(_selectedIndices, _newCards);

            await _panelView.PlayExitAsync(_selectedIndices, ct);

            for(int i = 0; i < _selectedIndices.Count; i++)
            {
                CardRuntime newCard = _newCards[i];
                if (newCard != null) _handLayoutView.ReplaceCardInstant(_selectedIndices[i], newCard);
            }

            await _panelView.PlayEnterAndCloseAsync(_selectedIndices, _newCards, ct);
        }
        finally
        {
            _panelView.OnCardClicked -= HandleCardClicked;
            _panelView.OnConfirmClicked -= HandleConfirmClicked;
            _confirmSource = null;

            if (ct.IsCancellationRequested) _panelView.HideImmediate();
            _mulliganController.Finish();
        }
    }

    private void HandleCardClicked(int handIndex)
    {
        MulliganToggleResult result = _mulliganController.Toggle(handIndex);
        if (result == MulliganToggleResult.Rejected) return;

        _panelView.SetCardSelected(handIndex, result == MulliganToggleResult.Selected);
        _panelView.SetSelectableCount(_mulliganController.RemainingSelectable);
    }

    private void HandleConfirmClicked()
    {
        _confirmSource?.TrySetResult();
    }
}
