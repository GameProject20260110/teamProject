using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public enum SkillMoveType
{
    None,       // 적 위치에서 바로 재생
    Projectile  // 플레이어 위치에서 날아감
}

public class Skill : MonoBehaviour
{
    [SerializeField] AudioClip sfxSound;
    [SerializeField] SkillMoveType moveType;
    [SerializeField] float moveSpeed = 10f;

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

        AudioManager.instance.PlaySfx(sfxSound);
        onHitCallback = ctx.onHit;
        onEndCallback = ctx.onEnd;
        ct = ctx.ct;

        if (moveType == SkillMoveType.Projectile)
        {
            transform.position = ctx.startPos;
            targetPosition = ctx.targetPos;

            Vector3 direction = (ctx.targetPos - ctx.startPos).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            isMoving = true;
        }
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