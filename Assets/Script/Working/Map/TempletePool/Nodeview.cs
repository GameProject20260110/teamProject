using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public enum MNodeType
{
    None,
    Battle,
    Shop,
    Event,
    Boss
}

public enum NodeState
{
    Locked,
    Available,
    Visited
}

public class NodeView : MonoBehaviour, IPointerClickHandler
{
    [Header("Refs")]
    [SerializeField] private Image background;
    [SerializeField] private Image icon;

    public event Action<NodeView> OnNodeClicked;

    public int NodeId { get; private set; }
    public NodeTypeData TypeData { get; private set; }
    public MNodeType Type => TypeData != null ? TypeData.Type : MNodeType.None;
    public NodeState State { get; private set; }

    private Tween idleTween;

    public void Init(int id, NodeTypeData typeData, NodeState initialState)
    {
        NodeId = id;
        TypeData = typeData;

        ApplyIcon();
        SetState(initialState);
    }

    private void ApplyIcon()
    {
        Sprite sprite = TypeData != null ? TypeData.Icon : null;
        icon.sprite = sprite;
        icon.enabled = sprite != null;
    }

    public void SetState(NodeState newState)
    {
        NodeState previousState = State;
        State = newState;

        switch (newState)
        {
            case NodeState.Locked:
                idleTween?.Kill();
                background.enabled = false;
                icon.enabled = false;
                SetInteractable(false);
                break;

            case NodeState.Available:
                background.enabled = true;
                ApplyIcon();
                SetVisualAlpha(1f);
                SetInteractable(true);

                if (previousState == NodeState.Locked)
                    PlayRevealAnimation();
                else
                    PlayIdleAnimation();
                break;

            case NodeState.Visited:
                background.enabled = true;
                SetVisualAlpha(1f);
                icon.enabled = false;
                idleTween?.Kill();
                SetInteractable(true);
                break;
        }
    }

    private UniTaskCompletionSource revealTcs;

    public UniTask WaitForRevealAsync()
    {
        return revealTcs != null ? revealTcs.Task : UniTask.CompletedTask;
    }

    private void PlayRevealAnimation()
    {
        revealTcs = new UniTaskCompletionSource();

        transform.localScale = Vector3.zero;
        transform
            .DOScale(1f, 0.35f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                PlayIdleAnimation();
                revealTcs.TrySetResult();
            });
    }

    private void SetVisualAlpha(float alpha)
    {
        var c = background.color;
        c.a = alpha;
        background.color = c;

        if (icon.enabled)
        {
            var ic = icon.color;
            ic.a = alpha;
            icon.color = ic;
        }
    }

    private void SetInteractable(bool value) // 이벤트 막기용
    {
        background.raycastTarget = value;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (State == NodeState.Locked) return;
        OnNodeClicked?.Invoke(this);
    }

    private void PlayIdleAnimation()
    {
        idleTween?.Kill();

        if (!icon.enabled) return;

        idleTween = icon.rectTransform
            .DOAnchorPosY(icon.rectTransform.anchoredPosition.y + 5f, 0.8f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnDestroy()
    {
        idleTween?.Kill();
    }
}