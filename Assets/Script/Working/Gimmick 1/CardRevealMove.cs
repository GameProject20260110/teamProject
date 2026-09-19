using System;
using DG.Tweening;
using UnityEngine;

public class CardRevealMove : MonoBehaviour
{
    [SerializeField] private Transform cardRoot;
    [SerializeField] private float moveDuration = 0.4f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;

    public bool IsMoving { get; private set; }
    public event Action OnMoveComplete;

    private Vector3 _homeLocalPosition;
    private Tween _tween;

    private void Awake()
    {
        if (cardRoot == null) cardRoot = transform;
        _homeLocalPosition = cardRoot.localPosition;
    }

    private void OnDestroy() => _tween?.Kill();

    public void Play(Vector3 fromWorldPosition, float startDelay = 0f)
    {
        _tween?.Kill();
        cardRoot.position = fromWorldPosition;
        IsMoving = true;

        _tween = cardRoot.DOLocalMove(_homeLocalPosition, moveDuration)
            .SetEase(moveEase)
            .SetDelay(startDelay)
            .OnComplete(HandleComplete);
    }

    public void SkipToEnd()
    {
        if (!IsMoving) return;
        _tween?.Kill();
        cardRoot.localPosition = _homeLocalPosition;
        HandleComplete();
    }

    public void ResetToStart(Vector3 fromWorldPosition)
    {
        _tween?.Kill();
        IsMoving = false;
        cardRoot.position = fromWorldPosition;
    }

    private void HandleComplete()
    {
        IsMoving = false;
        OnMoveComplete?.Invoke();
    }
}