using System.Collections.Generic;
using System;

public enum MulliganToggleResult 
{
    Selected,
    Deselected,
    Rejected
}

public class MulliganController
{
    private readonly DrawController _drawController;
    private readonly HandData _hand;
    private readonly HashSet<int> _selected = new HashSet<int>();
    private int _maxSelectable;

    public int SelectedCount => _selected.Count;
    public int MaxSelectable => _maxSelectable;
    public int RemainingSelectable => Math.Max(0, _maxSelectable - _selected.Count);

    public MulliganController(DrawController drawController, HandData hand)
    {
        _drawController = drawController;
        _hand = hand;
    }

    public void Begin()
    {
        _selected.Clear();
        _maxSelectable = _drawController != null ? Math.Min(_drawController.DrawPileCount, _hand.Count) : 0;
    }


    public MulliganToggleResult Toggle(int handIndex)
    {
        if (_drawController == null) return MulliganToggleResult.Rejected;
        if(handIndex < 0 || handIndex >= _hand.Count) return MulliganToggleResult.Rejected;

        if (_selected.Remove(handIndex)) return MulliganToggleResult.Deselected;
        if (_selected.Count >= _maxSelectable) return MulliganToggleResult.Rejected;

        _selected.Add(handIndex);
        return MulliganToggleResult.Selected;
    }

    public bool IsSelected(int handIndex) => _selected.Contains(handIndex);

    // 선택된 인덱스를 오름차순으로 채워줌
    public void GetSelectedIndices(List<int> result)
    {
        if (result == null) return;
        result.Clear();
        result.AddRange(_selected);
        result.Sort();
    }

    // 확인 버튼 - 선택된 자리 일괄 교체
    public int ApplyReplacements(IReadOnlyList<int> indices, List<CardRuntime> newCards)
    {
        if (_drawController == null) return 0;
        return _drawController.ReplaceHandCards(indices, newCards);
    }

    public void Finish()
    {
        _drawController?.ShuffleDrawPile();
        _selected.Clear();
    }
}
