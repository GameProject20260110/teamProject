using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandLayoutView : MonoBehaviour
{
    [SerializeField] private Transform handContainer;            
    [SerializeField] private Transform deckPileAnchor;           
    [SerializeField] private SpriteRenderer deckBoundsRenderer;  
    [SerializeField] private GameObject handCardPrefab;          
    [SerializeField] private HanddrawConfig config;

    [Header("메인 페이즈 - 카드 배치")]
    [SerializeField] private PlacementConfig placementConfig;    
    [SerializeField] private Camera targetCamera;                

    public event Action<HandCardView, CardPointerEventKind, PointerEventData> OnCardPointer;

    private readonly List<HandCardView> _activeCards = new List<HandCardView>();
    private Action<CardPointerView, CardPointerEventKind, PointerEventData> _pointerRelay; 
    private HandCardView _hoveredCard;   
    private HandCardView _excludedCard;  // 드래그로 빠져 있는 카드 - 목록엔 남아 있고 배치 계산에서만 제외

    private int _pullOutLayerId;
    private int _handLayerId;

    // 연속 드로우 배치 상태 - 여러 장이 동시에 날아가는 동안 부채꼴 기준 매수를 "최종 매수"로 고정해서
    // 각 카드가 처음부터 자기 최종 자리로 향하게 한다
    private int _layoutTotal;      
    private int _batchStartIndex;  
    private int _pendingIncoming;  
    private bool _roomScheduled;   

    private void Awake()
    {
        _pullOutLayerId = SortingLayerUtility.ResolveId(config != null ? config.pullOutSortingLayer : null, nameof(HandLayoutView));
        _handLayerId = SortingLayerUtility.ResolveId(config != null ? config.handSortingLayer : null, nameof(HandLayoutView));
        _pointerRelay = RelayPointer;
        if (targetCamera == null) targetCamera = Camera.main;
    }

    public int Count => _activeCards.Count;
    public int IndexOf(HandCardView cardView) => cardView == null ? -1 : _activeCards.IndexOf(cardView);

    // 연속 드로우 시작 전 1회 호출 - 이번에 몇 장 들어올지 미리 알려준다.
    public void BeginDrawBatch(int incomingCount)
    {
        if (incomingCount <= 0) return;

        _batchStartIndex = _activeCards.Count;
        _pendingIncoming = incomingCount;
        _layoutTotal = _activeCards.Count + incomingCount;
        _roomScheduled = false;
    }

    // 배치 종료(전체 도착 이후) - 예약 매수보다 덜 들어온 경우(취소 등) 빈 자리 없이 다시 정렬.
    public void EndDrawBatch()
    {
        bool hasUnfilledSlots = _pendingIncoming > 0 || _layoutTotal != _activeCards.Count;
        _pendingIncoming = 0;
        _roomScheduled = false;

        if (!hasUnfilledSlots) return;

        _layoutTotal = _activeCards.Count;
        float duration = config != null ? config.drawMoveDuration : 0.35f;
        for (int i = 0; i < _activeCards.Count; i++)
        {
            HandCardView cardView = _activeCards[i];
            if (cardView == null) continue; // 예외 방지

            (Vector2 pos, float rotZ) = GetCardSlot(i, _layoutTotal);
            cardView.MoveTo(pos, rotZ, 1f, duration);
        }
    }

    // 카드 1장 연출
    public async UniTask PlayDrawCardAsync(CardRuntime card, CancellationToken ct)
    {
        if (card == null || handCardPrefab == null || handContainer == null) return; // 예외 방지
        if (WorldPoolManager.instance == null)
        {
            Debug.LogWarning("[HandLayoutView] WorldPoolManager 인스턴스가 없습니다.");
            return;
        }

        if (_pendingIncoming <= 0) BeginDrawBatch(1); 

        Vector3 deckWorldPos = deckPileAnchor != null ? deckPileAnchor.position : handContainer.position;
        GameObject obj = WorldPoolManager.instance.Get(handCardPrefab, deckWorldPos, handContainer.rotation, handContainer);
        HandCardView cardView = obj != null ? obj.GetComponent<HandCardView>() : null;
        if (cardView == null)
        {
            Debug.LogWarning("[HandLayoutView] handCardPrefab에 HandCardView 컴포넌트가 없습니다.");
            if (obj != null) WorldPoolManager.instance.Return(obj);
            _pendingIncoming--; 
            return;
        }

        int pullOutOrder = config != null ? config.pullOutSortingOrder : -15;

        Subscribe(cardView);
        cardView.SetCard(card);
        cardView.SetFaceInstant(showFront: false);            
        cardView.SnapTo(GetDeckLocalPosition(), 0f, 1f);      
        cardView.SetSorting(_pullOutLayerId, pullOutOrder);   

        // 목적지/자리 벌리기 기준값은 출발 시점에 확정해서 로컬로 들고 감(다음 카드가 출발해도 이 값은 안 바뀜).
        int slotIndex = _activeCards.Count;
        int layoutTotal = _layoutTotal;
        int batchStartIndex = _batchStartIndex;
        bool scheduleMakeRoom = !_roomScheduled; 
        _roomScheduled = true;

        _activeCards.Add(cardView);
        _pendingIncoming--;

        // 1단계: 케이스 밑에서 옆으로 뽑아내기
        Vector2 pullOutOffset = GetPullOutOffset(cardView.LocalSize);
        float pullOutDuration = config != null ? config.pullOutDuration : 0.15f;
        await cardView.PlayPullOutAsync(pullOutOffset, pullOutDuration, ct);

        // 층 전환: 정렬값만 손패 레이어로 변경(같은 프레임, 위치 그대로)
        ApplyHandSorting(cardView, GetHandSortingOrder(slotIndex));

        // 2단계: 손패로 직선 이동 + 이동 도중 뒤집기 (+ 첫 카드면 기존 카드 자리 벌리기)
        await FlyIntoHandAsync(cardView, slotIndex, layoutTotal, batchStartIndex, scheduleMakeRoom, ct);
    }

    // 멀리건 교체 반영 - 자리/정렬/순서는 그대로 두고 카드 내용만 즉시 바꿈(멀리건 오버레이 뒤에서 호출됨).
    public void ReplaceCardInstant(int listIndex, CardRuntime newCard)
    {
        if (newCard == null || listIndex < 0 || listIndex >= _activeCards.Count) return;
        HandCardView cardView = _activeCards[listIndex];
        if (cardView == null) return;

        cardView.SetCard(newCard);
        cardView.SetFaceInstant(showFront: true);
    }

    // 손패 전체 공격력 표시 갱신(주사위 굴림 결과 반영)
    public void RefreshAllPower()
    {
        for (int i = 0; i < _activeCards.Count; i++)
        {
            if (_activeCards[i] != null) _activeCards[i].RefreshPower();
        }
    }

    // 묘지→덱 리셔플 시 덱 위치에 펀치스케일 연출
    public async UniTask PlayReshuffleAsync(CancellationToken ct)
    {
        if (deckPileAnchor == null) return;

        float duration = config != null ? config.reshuffleDuration : 0.3f;
        deckPileAnchor.DOKill(complete: true); 
        await deckPileAnchor.DOPunchScale(Vector3.one * 0.15f, duration, 4, 0.5f).WithCancellation(ct);
    }

    // 손패의 모든 카드를 풀에 반환하고 비운다
    public void ClearHand()
    {
        if (WorldPoolManager.instance != null)
        {
            foreach (HandCardView cardView in _activeCards)
            {
                if (cardView == null) continue;
                Unsubscribe(cardView);
                WorldPoolManager.instance.Return(cardView.gameObject);
            }
        }
        _activeCards.Clear();
        _hoveredCard = null;
        _excludedCard = null;

        // 배치 상태도 초기화
        _layoutTotal = 0;
        _batchStartIndex = 0;
        _pendingIncoming = 0;
        _roomScheduled = false;
    }

    #region 메인 페이즈 - Hover / 드래그 / 빼기·넣기 (CardPlacementOrchestrator가 호출)

    // Hover: 위로 hoverRisePixels + hoverScale 확대 + 맨 앞(order 가산) + 나머지 카드는 좌우로 벌어짐. 해제 시 원래 자리/정렬로.
    public void SetHovered(HandCardView cardView, bool hovered)
    {
        int index = IndexOf(cardView);
        if (index < 0 || cardView == _excludedCard) return;

        if (hovered)
        {
            if (_hoveredCard == cardView) return;
            _hoveredCard = cardView; // 이전 Hover 카드가 있었다면 아래 전체 재배치에서 원위치(Exit 누락 방어)
        }
        else
        {
            if (_hoveredCard != cardView) return;
            _hoveredCard = null;
        }

        RelayoutAll(HoverDuration); // 벌어짐 때문에 Hover 카드뿐 아니라 손패 전체 자리가 바뀜
    }

    // 드래그 시작: 카드는 목록에 남긴 채(배치 확정 전) 나머지 카드만 빈자리 없이 재정렬
    public bool BeginDragOut(HandCardView cardView)
    {
        if (IndexOf(cardView) < 0) return false;
        if (_hoveredCard == cardView) _hoveredCard = null;
        _excludedCard = cardView;
        RelayoutAll(RelayoutDuration);
        return true;
    }

    // 드래그 취소(무효 위치/규칙 거부): 원래 자리로 복귀 + 다른 카드도 다시 벌어짐.
    public void CancelDragOut(HandCardView cardView)
    {
        if (_excludedCard == cardView) _excludedCard = null;
        if (IndexOf(cardView) < 0) return;
        RelayoutAll(ReturnDuration);
    }

    // 손패에서 빼기(패널로 감) - 구독 해제, 남은 카드 order 재적용 + 재정렬
    public bool TakeCard(HandCardView cardView)
    {
        int index = IndexOf(cardView);
        if (index < 0) return false;

        _activeCards.RemoveAt(index);
        Unsubscribe(cardView);
        if (_excludedCard == cardView) _excludedCard = null;
        if (_hoveredCard == cardView) _hoveredCard = null;

        RelayoutAll(RelayoutDuration);
        return true;
    }

    // 손패에 넣기(패널에서 회수 / 교체로 밀려남)
    public void InsertCard(int listIndex, HandCardView cardView)
    {
        if (cardView == null || handContainer == null || IndexOf(cardView) >= 0) return;

        int clamped = Mathf.Clamp(listIndex, 0, _activeCards.Count);
        cardView.ReparentTo(handContainer);
        Subscribe(cardView);
        _activeCards.Insert(clamped, cardView);

        RelayoutAll(ReturnDuration); // 들어온 카드는 현재 위치에서 자기 자리로 이동, 나머지는 자리 벌림
    }

    // 손패 드롭 영역(handContainer 로컬 사각형) 안인지 - 놓인 카드를 손패로 회수할지 판정용.
    public bool IsInDropArea(Vector3 worldPos)
    {
        if (handContainer == null || placementConfig == null) return false; // 예외 방지
        Vector2 local = (Vector2)handContainer.InverseTransformPoint(worldPos) - placementConfig.handDropAreaCenter;
        Vector2 half = placementConfig.handDropAreaSize * 0.5f;
        return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y;
    }

    // 회수 카드가 들어갈 손패 목록 인덱스
    public int GetDropInsertIndex(Vector3 worldPos)
    {
        int total = _activeCards.Count;
        if (handContainer == null || total == 0) return 0;

        float localX = handContainer.InverseTransformPoint(worldPos).x;
        int leftCount = 0;
        for (int i = 0; i < total; i++)
        {
            var (pos, _) = GetCardSlot(i, total);
            if (pos.x < localX) leftCount++;
        }
        return total - leftCount;
    }

    // 전체 재정렬
    private void RelayoutAll(float duration)
    {
        for (int i = 0; i < _activeCards.Count; i++)
        {
            HandCardView cardView = _activeCards[i];
            if (cardView == null || cardView == _excludedCard) continue; // 드래그 중인 카드는 CardDragView가 위치 담당
            ApplyPose(cardView, i, duration);
        }
    }

    // 카드 1장 위치 적용: Hover 중이면 위로+확대+맨 앞, 아니면 부채꼴 자리.
    private void ApplyPose(HandCardView cardView, int listIndex, float duration)
    {
        if (cardView == null || listIndex < 0) return;

        (Vector2 pos, float rotZ) = GetLayoutSlot(listIndex);
        int order = GetHandSortingOrder(listIndex);
        float scale = 1f;

        bool hovered = cardView == _hoveredCard;
        if (hovered && placementConfig != null)
        {
            pos.y += GetHoverRiseLocal();
            scale = placementConfig.hoverScale;
            order += placementConfig.hoverOrderBoost;
        }
        else
        {
            pos.x += GetHoverSpreadOffset(listIndex);
        }

        ApplyHandSorting(cardView, order);
        cardView.MoveTo(pos, rotZ, scale, duration);
    }

    // 드래그로 빠진 카드를 제외한 배치 기준 자리
    private (Vector2 pos, float rotZ) GetLayoutSlot(int listIndex)
    {
        GetLayoutIndex(listIndex, out int index, out int total);
        return GetCardSlot(index, total);
    }

    // 배치 계산용 인덱스/장수(드래그로 빠진 카드 제외)
    private void GetLayoutIndex(int listIndex, out int index, out int total)
    {
        total = _activeCards.Count;
        index = listIndex;

        int excludedIndex = _excludedCard != null ? _activeCards.IndexOf(_excludedCard) : -1;
        if (excludedIndex >= 0)
        {
            total--;
            if (listIndex > excludedIndex) index--;
        }
        total = Mathf.Max(1, total);
    }

    // Hover 카드 기준 좌우 벌어짐(x 이동량). 화면상 왼쪽 카드는 왼쪽(-), 오른쪽 카드는 오른쪽(+)으로,
    private float GetHoverSpreadOffset(int listIndex)
    {
        if (placementConfig == null || _hoveredCard == null || placementConfig.hoverSpread <= 0f) return 0f;

        int hoveredListIndex = _activeCards.IndexOf(_hoveredCard);
        if (hoveredListIndex < 0 || hoveredListIndex == listIndex) return 0f;

        GetLayoutIndex(listIndex, out int index, out int total);
        GetLayoutIndex(hoveredListIndex, out int hoveredIndex, out _);
        int distance = ToVisualIndex(index, total) - ToVisualIndex(hoveredIndex, total); // 화면 순서 기준 거리(+ = 오른쪽)
        if (distance == 0) return 0f;

        float magnitude = placementConfig.hoverSpread * Mathf.Pow(placementConfig.hoverSpreadFalloff, Mathf.Abs(distance) - 1);
        return distance > 0 ? magnitude : -magnitude;
    }

    // 손패 정렬 + 입력 깊이(앞에 그려지는 카드일수록 카메라 쪽 -> 겹친 부분에서 앞 카드가 Hover/드래그를 받음).
    private void ApplyHandSorting(HandCardView cardView, int order)
    {
        cardView.SetSorting(_handLayerId, order);
        float depthStep = placementConfig != null ? placementConfig.raycastDepthPerOrder : 0f;
        cardView.SetRaycastDepth(-order * depthStep);
    }

    private float GetHoverRiseLocal()
    {
        if (placementConfig == null || handContainer == null) return 0f;
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null || Screen.height <= 0) return 0f; // 예외 방지

        float worldScreenHeight = cam.orthographic
            ? cam.orthographicSize * 2f
            : 2f * Mathf.Abs(handContainer.position.z - cam.transform.position.z) * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float world = placementConfig.hoverRisePixels * worldScreenHeight / Screen.height;
        return Mathf.Abs(MathUtility.SafeDivide(world, handContainer.lossyScale.y));
    }

    private float HoverDuration => placementConfig != null ? placementConfig.hoverDuration : 0.1f;
    private float RelayoutDuration => placementConfig != null ? placementConfig.handRelayoutDuration : 0.18f;
    private float ReturnDuration => placementConfig != null ? placementConfig.returnDuration : 0.2f;

    private void Subscribe(HandCardView cardView)
    {
        if (cardView == null || cardView.Pointer == null) return;
        cardView.Pointer.OnPointerEvent -= _pointerRelay;
        cardView.Pointer.OnPointerEvent += _pointerRelay;
    }

    private void Unsubscribe(HandCardView cardView)
    {
        if (cardView == null || cardView.Pointer == null) return;
        cardView.Pointer.OnPointerEvent -= _pointerRelay;
    }

    private void RelayPointer(CardPointerView pointer, CardPointerEventKind kind, PointerEventData eventData)
    {
        if (pointer == null || pointer.Card == null) return;
        OnCardPointer?.Invoke(pointer.Card, kind, eventData);
    }

    #endregion

    // 직선 이동 / 지연 뒤집기 / (첫 카드면) 지연 자리 벌리기를 동시 진행 - 셋 다 끝나면 이 카드는 "도착".
    private async UniTask FlyIntoHandAsync(HandCardView cardView, int slotIndex, int layoutTotal, int batchStartIndex, bool scheduleMakeRoom, CancellationToken ct)
    {
        (Vector2 targetPos, float rotZ) = GetCardSlot(slotIndex, layoutTotal);

        float moveDuration = config != null ? config.drawMoveDuration : 0.35f;
        float flipDuration = config != null ? config.flipDuration : 0.25f;
        float flipStartRatio = config != null ? config.flipStartRatio : 0.2f;
        float makeRoomStartRatio = config != null ? config.makeRoomStartRatio : 0.4f;
        AnimationCurve moveCurve = config != null ? config.drawMoveCurve : null;

        UniTask moveTask = cardView.PlayDrawMoveAsync(targetPos, rotZ, 1f, moveDuration, moveCurve, ct);
        UniTask flipTask = PlayFlipDelayedAsync(cardView, moveDuration * flipStartRatio, flipDuration, ct);
        UniTask makeRoomTask = scheduleMakeRoom
            ? MakeRoomDelayedAsync(moveDuration * makeRoomStartRatio, moveDuration * (1f - makeRoomStartRatio), batchStartIndex, layoutTotal, ct)
            : UniTask.CompletedTask;

        await UniTask.WhenAll(moveTask, flipTask, makeRoomTask);
    }

    // 손패로 이동하는 도중 지정 시점에 뒤집기 시작
    private static async UniTask PlayFlipDelayedAsync(HandCardView cardView, float delay, float flipDuration, CancellationToken ct)
    {
        if (delay > 0f) await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct);
        await cardView.PlayFlipAsync(flipDuration, ct);
    }

    // 배치의 첫 카드가 일정 지점까지 날아오면, 배치 이전부터 있던 카드들만 최종 매수 기준 자리로 벌어짐
    private async UniTask MakeRoomDelayedAsync(float delay, float remainingDuration, int existingCount, int layoutTotal, CancellationToken ct)
    {
        if (delay > 0f) await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct);

        int count = Mathf.Min(existingCount, _activeCards.Count); // 예외 방지 - 그 사이 ClearHand된 경우
        for (int i = 0; i < count; i++)
        {
            HandCardView cardView = _activeCards[i];
            if (cardView == null) continue; // 예외 방지(풀 반환 등으로 참조가 끊긴 경우)

            (Vector2 pos, float rotZ) = GetCardSlot(i, layoutTotal);
            cardView.MoveTo(pos, rotZ, 1f, remainingDuration);
        }
    }

    // 손패 정렬 order
    private int GetHandSortingOrder(int listIndex)
    {
        int baseOrder = config != null ? config.handSortingOrderBase : 100;
        return baseOrder - listIndex;
    }

    // 덱 위치를 handContainer 로컬 좌표로 변환
    private Vector2 GetDeckLocalPosition()
    {
        if (deckPileAnchor == null || handContainer == null) return Vector2.zero;
        return handContainer.InverseTransformPoint(deckPileAnchor.position);
    }

    // 덱 케이스의 크기를 handContainer 로컬 유닛으로 변환
    private Vector2 GetDeckLocalSize()
    {
        if (deckBoundsRenderer == null || handContainer == null) return Vector2.zero; // 예외 방지

        Vector3 worldSize = deckBoundsRenderer.bounds.size;
        Vector3 containerScale = handContainer.lossyScale;
        return new Vector2(Mathf.Abs(MathUtility.SafeDivide(worldSize.x, containerScale.x)),
                           Mathf.Abs(MathUtility.SafeDivide(worldSize.y, containerScale.y)));
    }

    // 1단계 이동량
    // 방향 d로 이동할 때 두 사각형이 겹치지 않는 최소 거리 = 두 사각형의 d 방향 반폭 합(|dx|·반너비 + |dy|·반높이).
    private Vector2 GetPullOutOffset(Vector2 cardLocalSize)
    {
        Vector2 baseOffset = config != null ? config.pullOutOffset : new Vector2(-1.6f, 0f);
        if (config == null || !config.autoPullOutDistance) return baseOffset;

        Vector2 dir = baseOffset.sqrMagnitude > 0f ? baseOffset.normalized : Vector2.left; // 예외 방지 - 0벡터면 왼쪽
        Vector2 cardHalf = cardLocalSize * 0.5f;
        Vector2 deckHalf = GetDeckLocalSize() * 0.5f;

        float distance = Mathf.Abs(dir.x) * (cardHalf.x + deckHalf.x)
                       + Mathf.Abs(dir.y) * (cardHalf.y + deckHalf.y)
                       + config.pullOutMargin;
        return dir * distance;
    }

    // 카드 목록 순서
    // 덱이 손패 왼쪽에 있으므로 새로 뽑은 카드가 항상 맨 왼쪽에 들어감
    private (Vector2 pos, float rotZ) GetCardSlot(int listIndex, int total)
    {
        return GetFanSlot(ToVisualIndex(listIndex, total), total);
    }

    public static int ToVisualIndex(int listIndex, int total) => total - 1 - listIndex;

    // 좌우 대칭 부채꼴 좌표/회전 계산(handContainer 로컬)
    // 카드 간격 = min(카드당 최대 간격, 전체 최대 각도 / (장수-1)) -> 적을 땐 가운데로 모이고, 많을 땐 전체 폭 안으로 압축.
    private (Vector2 pos, float rotZ) GetFanSlot(int index, int total)
    {
        float radius = config != null ? config.fanRadius : 14f;
        float maxAngle = config != null ? config.maxFanAngle : 18f;
        float maxStep = config != null ? config.maxCardStepAngle : 4f;

        float step = total <= 1 ? 0f : Mathf.Min(maxStep, maxAngle / (total - 1));
        float angle = (index - (total - 1) * 0.5f) * step;
        float rad = angle * Mathf.Deg2Rad;

        Vector2 pos = new Vector2(radius * Mathf.Sin(rad), -radius * (1f - Mathf.Cos(rad)));
        return (pos, -angle);
    }
}