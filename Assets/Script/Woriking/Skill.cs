using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public enum SkillMoveType
{
    None,
    Projectile,
    Particle
}

public class Skill : MonoBehaviour
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

    public void Init(SkillContext ctx)
    {
        onHitCallback = null;
        onEndCallback = null;
        isMoving = false;

        AudioManager.instance.PlaySfx(sfxKey);
        onHitCallback = ctx.onHit;
        onEndCallback = ctx.onEnd;
        ct = ctx.ct;

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
                // 애니메이션 이벤트로 타이밍 잡음
                break;
        }

        Debug.Log($"{transform.position}, {targetPosition}, {moveSpeed}");
    }

    private void Update()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

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
        await UniTask.Delay(100, cancellationToken: ct);
        OnAttackEnd();
    }

    private async UniTaskVoid PlayParticleAsync(ParticleSystem particle, CancellationToken ct)
    {
        HitAnimFrame();
        await UniTask.WaitUntil(() => !particle.isPlaying, cancellationToken: ct);
        OnAttackEnd();
    }

    // Animation Event (None 타입에서 사용)
    public void HitAnimFrame()
    {
        onHitCallback?.Invoke();
    }

    // Animation Event
    public void OnAttackEnd()
    {
        onEndCallback?.Invoke();
        ObjectPool.instance.Return(gameObject);
    }
}