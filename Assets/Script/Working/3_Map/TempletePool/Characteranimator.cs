using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimator : MonoBehaviour
{
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int PlayIdleVariantHash = Animator.StringToHash("Idle2");

    [SerializeField] private Animator animator;

    [Header("Idle2")]
    [SerializeField] private float idleVariantInterval = 10f;

    [Header("Idle Transition")]
    [SerializeField] private float idleTransitionDelay = 0.05f;

    private float idleTimer;
    private bool isMoving;
    private CancellationTokenSource idleDelayCts;

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    public void SetMoving(bool moving)
    {
        isMoving = moving;
        CancelPendingIdle();

        if (moving)
        {
            idleTimer = 0f;
            animator.SetBool(IsMovingHash, true);
        }
        else
        {
            idleDelayCts = new CancellationTokenSource();
            DelayedIdleAsync(idleDelayCts.Token).Forget();
        }
    }

    private void CancelPendingIdle()
    {
        idleDelayCts?.Cancel();
        idleDelayCts?.Dispose();
        idleDelayCts = null;
    }

    private async UniTaskVoid DelayedIdleAsync(CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(idleTransitionDelay), cancellationToken: token);
        animator.SetBool(IsMovingHash, false);
    }

    private void Update()
    {
        if (isMoving) return;

        idleTimer += Time.deltaTime;

        if (idleTimer >= idleVariantInterval)
        {
            idleTimer = 0f;
            animator.SetTrigger(PlayIdleVariantHash);
        }
    }

    private void OnDestroy()
    {
        CancelPendingIdle();
    }
}