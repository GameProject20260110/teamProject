using UnityEngine;
using DG.Tweening;

/// <summary>
/// 요정/나비 정령 한 마리의 동작을 담당.
/// - 날갯짓 자체는 Animator(AnimationClip)로 재생 (프레임 수동 처리 없음)
/// - 지정된 영역(bounds) 안에서 랜덤한 지점을 골라 곡선 이동(Ease)으로 떠다님
/// - 도착하면 잠깐 머물렀다가 다음 랜덤 지점으로 이동 (무한 반복)
///
/// FairySpiritSpawner가 Init()에서 AnimatorOverrideController를 만들어 넘겨줌으로써
/// 색상 변형(클립)만 바꿔 끼우는 구조.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class FairySpirit : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Animator _animator;

    private float _leftX, _rightX, _bottomY, _topY;
    private float _minSpeed, _maxSpeed;
    private float _minWait, _maxWait;

    private Tween _moveTween;
    private bool _initialized;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// runtimeController: 스포너가 색상 클립을 오버라이드해서 만들어준 컨트롤러
    /// </summary>
    public void Init(
        RuntimeAnimatorController runtimeController,
        Bounds wanderBounds,
        float minSpeed,
        float maxSpeed,
        float minWait,
        float maxWait)
    {
        if (runtimeController != null)
        {
            _animator.runtimeAnimatorController = runtimeController;
        }

        _leftX = wanderBounds.min.x;
        _rightX = wanderBounds.max.x;
        _bottomY = wanderBounds.min.y;
        _topY = wanderBounds.max.y;

        _minSpeed = minSpeed;
        _maxSpeed = maxSpeed;
        _minWait = minWait;
        _maxWait = maxWait;

        transform.position = RandomPointInBounds();

        _initialized = true;
        MoveToNextPoint();
    }

    private Vector3 RandomPointInBounds()
    {
        float x = Random.Range(_leftX, _rightX);
        float y = Random.Range(_bottomY, _topY);
        return new Vector3(x, y, transform.position.z);
    }

    private void MoveToNextPoint()
    {
        if (!_initialized) return;

        Vector3 target = RandomPointInBounds();

        float distance = Vector3.Distance(transform.position, target);
        float speed = Random.Range(_minSpeed, _maxSpeed);
        float duration = Mathf.Max(distance / Mathf.Max(speed, 0.01f), 0.4f);

        // 이동 방향에 따라 좌우 반전 (원본 스프라이트가 오른쪽을 보고 있다는 가정, 아니면 조건 반대로)
        bool movingRight = target.x > transform.position.x;
        _sr.flipX = !movingRight;

        _moveTween?.Kill();
        _moveTween = transform
            .DOMove(target, duration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                float wait = Random.Range(_minWait, _maxWait);
                DOVirtual.DelayedCall(wait, MoveToNextPoint);
            });
    }

    private void OnDestroy()
    {
        _moveTween?.Kill();
        DOTween.Kill(transform);
    }
}
