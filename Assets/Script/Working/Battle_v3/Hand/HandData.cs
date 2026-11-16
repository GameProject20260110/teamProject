using System;
using System.Collections.Generic;
using UnityEngine;

public class HandData
{
    private readonly List<CardRuntime> _cards = new List<CardRuntime>();

    public IReadOnlyList<CardRuntime> Cards => _cards;
    public int Count => _cards.Count;
    public CardRuntime this[int index] => index >= 0 && index < _cards.Count ? _cards[index] : null;

    public event Action OnHandChanged;

    public int IndexOf(CardRuntime card) => card == null ? -1 : _cards.IndexOf(card);
    public bool Contains(CardRuntime card) => card != null && _cards.Contains(card);

    // 맨 뒤에 추가(드로우)
    // 새 카드는 화면상 맨 왼쪽
    public bool Add(CardRuntime card)
    {
        if (card == null || _cards.Contains(card)) return false;
        _cards.Add(card);
        OnHandChanged?.Invoke();
        return true;
    }

    // 빼기
    public bool RemoveAt(int index)
    {
        if(index < 0 || index >= _cards.Count) return false;
        _cards.RemoveAt(index);
        OnHandChanged?.Invoke();
        return true;
    }

    // 같은 자리 카드 바꾸기
    public bool SetAt(int index, CardRuntime card)
    {
        if(card == null || index < 0 || index >= _cards.Count) return false;
        if (_cards[index] == card) return true;
        if (_cards.Contains(card)) return false;
        _cards[index] = card;
        OnHandChanged?.Invoke();
        return true;
    }

    // 넣기
    public int Insert(int index, CardRuntime card)
    {
        if(card == null || _cards.Contains(card)) return -1;
        int clamped = Mathf.Max(0, Mathf.Min(index, _cards.Count));
        _cards.Insert(clamped, card);
        OnHandChanged?.Invoke();
        return clamped;
    }

    public void Clear()
    {
        if (_cards.Count == 0) return;
        _cards.Clear();
        OnHandChanged?.Invoke();
    }
}
