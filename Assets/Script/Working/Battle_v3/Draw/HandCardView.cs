using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

// 손패/패널 카드 1장의 시각 표현(월드 SpriteRenderer 기준)만 담당한다. 위치/회전/크기/정렬 지시는 전부
public class HandCardView : MonoBehaviour, IPoolCallbackReceiver
{
    [SerializeField] private CardUI cardUI;                 
    [SerializeField] private Transform flipRoot;            
    [SerializeField] private GameObject frontFace;          
    [SerializeField] private GameObject backFace;           
    [SerializeField] private SpriteRenderer sizeReference;  
    [SerializeField] private SortingGroup sortingGroup;     
    [SerializeField] private CardPointerView pointer;       

    private Vector3 _baseScale = Vector3.one;     
    private Vector3 _flipBaseScale = Vector3.one;
    private float _depthZ;                         

    public CardRuntime Runtime => cardUI != null ? cardUI.Runtime : null;
    public CardPointerView Pointer => pointer;

    private void Awake()
    {
        _baseScale = transform.localScale;
        if (flipRoot != null) _flipBaseScale = flipRoot.localScale;

        
        if (sortingGroup == null && !TryGetComponent(out sortingGroup))
            sortingGroup = gameObject.AddComponent<SortingGroup>();
        if (pointer == null) TryGetComponent(out pointer);
    }

    #region 크기 / 정렬 / 내용

    // 부모 로컬 유닛 기준 "현재" 카드 크기
    public Vector2 LocalSize
    {
        get
        {
            if (sizeReference == null || sizeReference.sprite == null) return Vector2.zero; // 예외 방지

            Vector2 raw = GetRawSpriteSize();
            Vector3 refScale = sizeReference.transform.lossyScale;
            Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            return new Vector2(raw.x * Mathf.Abs(MathUtility.SafeDivide(refScale.x, parentScale.x)),
                               raw.y * Mathf.Abs(MathUtility.SafeDivide(refScale.y, parentScale.y)));
        }
    }

    // 배율 scale일 때의 카드 크기(부모 로컬 유닛). 지금 Hover/드래그로 커져 있어도 그 영향 없이 계산
    public Vector2 GetLocalSizeAtScale(float scale)
    {
        if (sizeReference == null || sizeReference.sprite == null) return Vector2.zero; // 예외 방지

        Vector2 raw = GetRawSpriteSize();
        Vector3 refScale = sizeReference.transform.lossyScale;
        Vector3 rootScale = transform.lossyScale;
        // 루트 대비 기준 스프라이트의 상대 배율(프리팹 내부 구조값 - 루트 크기가 변해도 일정)
        float rx = Mathf.Abs(MathUtility.SafeDivide(refScale.x, rootScale.x));
        float ry = Mathf.Abs(MathUtility.SafeDivide(refScale.y, rootScale.y));
        return new Vector2(raw.x * rx * Mathf.Abs(_baseScale.x) * scale,
                           raw.y * ry * Mathf.Abs(_baseScale.y) * scale);
    }

    private Vector2 GetRawSpriteSize()
    {
        return sizeReference.drawMode == SpriteDrawMode.Simple
            ? (Vector2)sizeReference.sprite.bounds.size
            : sizeReference.size; // Sliced/Tiled는 그려지는 크기가 sprite.bounds와 다름
    }

    // 정렬 지정 - 덱 케이스 밑 <-> 손패 <-> 드래그 <-> 패널 전환에 사용
    public void SetSorting(int sortingLayerId, int sortingOrder)
    {
        if (sortingGroup == null) return; // 예외 방지
        sortingGroup.sortingLayerID = sortingLayerId;
        sortingGroup.sortingOrder = sortingOrder;
    }

    // 포인터 입력 우선순위용 로컬 z(카메라에 가까울수록 = 음수일수록 먼저 맞음) 
    public void SetRaycastDepth(float localZ)
    {
        _depthZ = localZ;
        Vector3 p = transform.localPosition;
        p.z = localZ;
        transform.localPosition = p;
    }

    public void SetCard(CardRuntime runtime)
    {
        if (runtime == null || cardUI == null) return; 
        cardUI.SetUp(runtime);
    }

    // 공격력 숫자만 다시 표시(주사위 굴림/효과로 값이 바뀐 뒤)
    public void RefreshPower()
    {
        if (cardUI != null) cardUI.RefreshPower();
    }

    // 부모 교체(손패 <-> 패널). 월드 위치/크기를 유지한 채 바꾼 뒤, 이어지는 이동 트윈이 새 부모 기준 목표로 이동.
    public void ReparentTo(Transform parent)
    {
        if (parent == null || transform.parent == parent) return;
        transform.SetParent(parent, true);
        SetRaycastDepth(_depthZ);
    }

    #endregion

    #region 드로우

    // 트윈 없이 즉시 앞/뒷면 상태를 지정(드로우 시작 시 뒷면으로 세팅하는 용도)
    public void SetFaceInstant(bool showFront)
    {
        if (frontFace != null) frontFace.SetActive(showFront);
        if (backFace != null) backFace.SetActive(!showFront);
        if (flipRoot != null) flipRoot.localScale = _flipBaseScale;
    }

    // 1단계: 현재 위치에서 localOffset만큼 빠져나옴(뒷면 유지). 덱 케이스가 가려주므로 페이드 불필요.
    public async UniTask PlayPullOutAsync(Vector2 localOffset, float duration, CancellationToken ct)
    {
        Vector3 target = transform.localPosition + (Vector3)localOffset;
        if (duration <= 0f)
        {
            transform.localPosition = target;
            return;
        }

        transform.DOKill();
        await transform.DOLocalMove(target, duration).SetEase(Ease.OutQuad).WithCancellation(ct);
    }

    // 뒷면→앞면 X스케일 뒤집기 1회 재생(1→0에서 면 교체→0→1). 언제 시작할지는 HandLayoutView가 결정(이동 도중 재생).
    public async UniTask PlayFlipAsync(float duration, CancellationToken ct)
    {
        if (flipRoot == null || duration <= 0f) { SetFaceInstant(true); return; } // 예외 방지 - 즉시 앞면

        float half = duration * 0.5f;
        flipRoot.DOKill();
        await flipRoot.DOScaleX(0f, half).SetEase(Ease.InQuad).WithCancellation(ct);

        // 스케일 0(옆에서 보면 안 보이는 시점)에서 면을 교체 - 뒤집기 중간 지점.
        if (frontFace != null) frontFace.SetActive(true);
        if (backFace != null) backFace.SetActive(false);

        await flipRoot.DOScaleX(_flipBaseScale.x, half).SetEase(Ease.OutQuad).WithCancellation(ct);
    }

    // 덱→손패 진입 전용 직선 이동. 목적지는 처음부터 "자기 최종 부채꼴 자리"라 도중에 목표가 안 바뀜.
    public async UniTask PlayDrawMoveAsync(Vector2 targetLocalPos, float rotationZ, float scale, float duration, AnimationCurve moveCurve, CancellationToken ct)
    {
        if (duration <= 0f) { SnapTo(targetLocalPos, rotationZ, scale); return; }

        transform.DOKill();

        Tweener move = transform.DOLocalMove(ToLocal(targetLocalPos), duration);
        if (moveCurve != null) move.SetEase(moveCurve);
        else move.SetEase(Ease.OutQuad); // 커브 미할당 시 방어적 기본 Ease

        transform.DOLocalRotate(new Vector3(0f, 0f, rotationZ), duration).SetEase(Ease.OutQuad);
        transform.DOScale(_baseScale * scale, duration).SetEase(Ease.OutQuad);

        await move.WithCancellation(ct); // 위치 트윈 완료 = 도착 기준
    }

    #endregion

    #region 이동 (재정렬 / Hover / 패널 안착)

    // 위치를 즉시 지정
    public void SnapTo(Vector2 localPos, float rotationZ, float scale)
    {
        transform.DOKill();
        transform.localPosition = ToLocal(localPos);
        transform.localEulerAngles = new Vector3(0f, 0f, rotationZ);
        transform.localScale = _baseScale * scale;
    }

    // 자리 벌리기/재정렬/Hover 전용 이동
    public void MoveTo(Vector2 localPos, float rotationZ, float scale, float duration, Ease ease = Ease.OutQuad)
    {
        if (duration <= 0f) { SnapTo(localPos, rotationZ, scale); return; }
        StartMove(localPos, rotationZ, scale, duration, ease);
    }

    // 도착을 기다려야 하는 이동
    public UniTask MoveToAsync(Vector2 localPos, float rotationZ, float scale, float duration, Ease ease, CancellationToken ct)
    {
        if (duration <= 0f) { SnapTo(localPos, rotationZ, scale); return UniTask.CompletedTask; }
        return StartMove(localPos, rotationZ, scale, duration, ease).WithCancellation(ct);
    }

    private Tweener StartMove(Vector2 localPos, float rotationZ, float scale, float duration, Ease ease)
    {
        transform.DOKill();
        Tweener move = transform.DOLocalMove(ToLocal(localPos), duration).SetEase(ease);
        transform.DOLocalRotate(new Vector3(0f, 0f, rotationZ), duration).SetEase(ease);
        transform.DOScale(_baseScale * scale, duration).SetEase(ease);
        return move;
    }

    // 패널 안착 순간 짧은 튕김
    public UniTask PlayPunchAsync(float punchRatio, float duration, CancellationToken ct)
    {
        if (punchRatio <= 0f || duration <= 0f) return UniTask.CompletedTask;
        return transform.DOPunchScale(transform.localScale * punchRatio, duration, 1, 0f).WithCancellation(ct);
    }

    #endregion

    #region 드래그

    // 집는 순간: 진행 중인 이동/튕김을 끊고 회전 0 + 확대.
    public void PlayDragPose(float scale, float duration)
    {
        transform.DOKill();
        if (duration <= 0f)
        {
            transform.localEulerAngles = Vector3.zero;
            transform.localScale = _baseScale * scale;
            return;
        }
        transform.DOLocalRotate(Vector3.zero, duration).SetEase(Ease.OutQuad);
        transform.DOScale(_baseScale * scale, duration).SetEase(Ease.OutQuad);
    }

    // 드래그 중 위치 지정(월드 좌표)
    public void SetWorldPosition(Vector3 worldPos)
    {
        Vector3 local = transform.parent != null ? transform.parent.InverseTransformPoint(worldPos) : worldPos;
        local.z = _depthZ;
        transform.localPosition = local;
    }

    #endregion

    private Vector3 ToLocal(Vector2 pos) => new Vector3(pos.x, pos.y, _depthZ);

    public void OnRent() { }

    public void OnReturn()
    {
        transform.DOKill();
        if (flipRoot != null) flipRoot.DOKill();
        _depthZ = 0f;
        SetFaceInstant(true);
    }
}