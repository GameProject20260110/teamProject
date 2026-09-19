using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public enum SkillMoveType
{
    None,
    Projectile,
    Particle
}

public class Skill : MonoBehaviour, IPoolCallbackReceiver
{
    [SerializeField] float spriteBaseAngle = 0f;
    [SerializeField] SkillMoveType moveType;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] private string sfxKey;

    private System.Action onHitCallback;
    private System.Action onEndCallback;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private CancellationToken ct;

    public void OnRent()
    {
        isMoving = false;
        onHitCallback = null;
        onEndCallback = null;
    }

    public void OnReturn()
    {
        var particle = GetComponent<ParticleSystem>();
        if (particle != null)
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void Init(SkillContext ctx)
    {
        onHitCallback = ctx.onHit;
        onEndCallback = ctx.onEnd;
        ct = ctx.ct;

        AudioManager.Instance.PlaySfx(sfxKey);

        switch (moveType)
        {
            case SkillMoveType.Projectile:
                transform.position = ctx.startPos;
                targetPosition = ctx.targetPos;
                Vector3 direction = (ctx.targetPos - ctx.startPos).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle - spriteBaseAngle);
                isMoving = true;
                break;

            case SkillMoveType.Particle:
                transform.position = ctx.targetPos;
                var particle = GetComponent<ParticleSystem>();
                if (particle != null)
                {
                    particle.Play();
                    PlayParticleAsync(particle, ct).Forget();
                }
                break;

            case SkillMoveType.None:
                break;
        }
    }

    private void Update()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            isMoving = false;
            transform.position = targetPosition;
            OnArrivedAsync(ct).Forget();
        }
    }

    private async UniTaskVoid OnArrivedAsync(CancellationToken ct)
    {
        HitAnimFrame();
        try
        {
            await UniTask.Delay(100, cancellationToken: ct);
        }
        catch (OperationCanceledException) { }
        finally
        {
            OnAttackEnd();
        }
    }

    private async UniTaskVoid PlayParticleAsync(ParticleSystem particle, CancellationToken ct)
    {
        HitAnimFrame();
        try
        {
            await UniTask.WaitUntil(() => !particle.isPlaying, cancellationToken: ct);
        }
        catch (OperationCanceledException) { }
        finally
        {
            OnAttackEnd();
        }
    }

    public void HitAnimFrame()
    {
        onHitCallback?.Invoke();
    }

    public void OnAttackEnd()
    {
        onEndCallback?.Invoke();
        WorldPoolManager.instance.Return(gameObject);
    }
}