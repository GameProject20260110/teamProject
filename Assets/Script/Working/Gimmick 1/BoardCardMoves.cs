using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardCardMoves : MonoBehaviour
{
    [SerializeField] private List<CardRevealMove> cards = new List<CardRevealMove>();

    [SerializeField] private Transform dispenserExitPoint;

    [SerializeField] private float staggerDelay = 0.12f;
    [SerializeField] private float initialDelay = 0f;

    public event Action OnAllMoved;

    private int _pendingCount;

    [ContextMenu("Play All")]
    public void PlayAll()
    {
        _pendingCount = 0;

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (card == null) continue;

            _pendingCount++;
            card.OnMoveComplete -= HandleOneMoved;
            card.OnMoveComplete += HandleOneMoved;
            card.Play(dispenserExitPoint.position, initialDelay + staggerDelay * i);
        }
    }

    public void SkipAll()
    {
        foreach (var card in cards)
        {
            if (card != null && card.IsMoving) card.SkipToEnd();
        }
    }

    public void ResetAll()
    {
        foreach (var card in cards)
        {
            if (card != null) card.ResetToStart(dispenserExitPoint.position);
        }
    }

    private void HandleOneMoved()
    {
        _pendingCount--;
        if (_pendingCount <= 0) OnAllMoved?.Invoke();
    }
}
