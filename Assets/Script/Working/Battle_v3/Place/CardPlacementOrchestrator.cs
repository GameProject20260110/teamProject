using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

// 메인 페이즈(카드 패널 배치) 지휘

public class CardPlacementOrchestrator
{
    private enum DragSource { None, Hand, Field }
    private enum DropTarget { None, Slot, Hand }

    private readonly HandData _hand;
    private readonly CardPlacementController _placementController;
    private readonly HandLayoutView _handLayoutView;
    private readonly FieldBoardView _fieldBoardView;
    private readonly CardDragView _dragView;

    // 구독용 델리게이트 1회 캐싱
    private readonly Action<HandCardView, CardPointerEventKind, PointerEventData> _handPointerHandler;
    private readonly Action<HandCardView, CardPointerEventKind, PointerEventData> _fieldPointerHandler;
    private readonly Action _dragAbortedHandler;

    private UniTaskCompletionSource _endSource;
    private bool _isRunning;

    // 진행 중인 드래그 1건
    private HandCardView _dragCard;
    private DragSource _dragSource;
    private int _dragFromSlot = -1;
    private int _dragPointerId;

    // 누르고 있는 카드(드래그 시작 전 - 판정 거리 이내)
    private HandCardView _pressedCard;
    private int _pressPointerId;

    // Hover 중인 손패 카드 1장
    private HandCardView _hoverCard;

    public bool IsRunning => _isRunning;

    public CardPlacementOrchestrator(HandData hand, CardPlacementController placementController,
                                     HandLayoutView handLayoutView, FieldBoardView fieldBoardView, CardDragView dragView)
    {
        _hand = hand;
        _placementController = placementController;
        _handLayoutView = handLayoutView;
        _fieldBoardView = fieldBoardView;
        _dragView = dragView;

        _handPointerHandler = (card, kind, e) => HandlePointer(card, DragSource.Hand, kind, e);
        _fieldPointerHandler = (card, kind, e) => HandlePointer(card, DragSource.Field, kind, e);
        _dragAbortedHandler = CancelActiveDrag;
    }

    // 배치 페이즈 실행 - 배치 완료 요청(RequestEndPlacement)이 올 때까지 입력을 받는다.
    public async UniTask RunAsync(CancellationToken ct)
    {
        if (_isRunning) return;
        if (_hand == null || _placementController == null || _handLayoutView == null || _fieldBoardView == null || _dragView == null)
        {
            Debug.LogWarning("[CardPlacementOrchestrator] 연결 누락 - 배치 페이즈를 건너뜁니다.");
            return;
        }
        if (_fieldBoardView.SlotCount != _placementController.SlotCount)
            Debug.LogWarning($"[CardPlacementOrchestrator] 패널 칸 수 불일치(View {_fieldBoardView.SlotCount} / Controller {_placementController.SlotCount}).");

        _isRunning = true;
        _endSource = new UniTaskCompletionSource();
        _handLayoutView.OnCardPointer += _handPointerHandler;
        _fieldBoardView.OnCardPointer += _fieldPointerHandler;
        _dragView.OnDragAborted += _dragAbortedHandler;

        try
        {
            await _endSource.Task.AttachExternalCancellation(ct);
        }
        finally
        {
            CancelActiveDrag(); // 들고 있던 카드는 원래 자리로
            ClearPress();
            ClearHover();
            _handLayoutView.OnCardPointer -= _handPointerHandler;
            _fieldBoardView.OnCardPointer -= _fieldPointerHandler;
            _dragView.OnDragAborted -= _dragAbortedHandler;
            _endSource = null;
            _isRunning = false;
        }
    }

    // 배치 완료
    public void RequestEndPlacement()
    {
        _endSource?.TrySetResult();
    }

    #region 입력 분기

    private void HandlePointer(HandCardView card, DragSource source, CardPointerEventKind kind, PointerEventData eventData)
    {
        if (!_isRunning || card == null) return;

        switch (kind)
        {
            case CardPointerEventKind.Enter:
                if (_dragCard == null && source == DragSource.Hand) SetHover(card, true); // 드래그 중엔 Hover 무시
                break;
            case CardPointerEventKind.Exit:
                if (_dragCard == null && source == DragSource.Hand) SetHover(card, false);
                break;
            case CardPointerEventKind.Down:
                Press(card, source, eventData);
                break;
            case CardPointerEventKind.Up:
                Release(card, eventData);
                break;
            case CardPointerEventKind.BeginDrag:
                BeginDrag(card, source, eventData);
                break;
            case CardPointerEventKind.Drag:
                if (IsActiveDrag(card, eventData)) UpdateDrag(eventData);
                break;
            case CardPointerEventKind.EndDrag:
                if (IsActiveDrag(card, eventData)) EndDrag(eventData);
                break;
        }
    }

    private bool IsActiveDrag(HandCardView card, PointerEventData eventData)
    {
        return card == _dragCard && (eventData == null || eventData.pointerId == _dragPointerId);
    }

    #endregion

    #region Hover

    // 손패 카드 Hover만 처리(HandLayoutView가 이전 Hover 카드 원위치까지 함께 처리).
    private void SetHover(HandCardView card, bool hovered)
    {
        if (hovered) _hoverCard = card;
        else if (_hoverCard == card) _hoverCard = null;
        _handLayoutView.SetHovered(card, hovered);
    }

    private void ClearHover()
    {
        if (_hoverCard == null) return;
        HandCardView card = _hoverCard;
        _hoverCard = null;
        _handLayoutView.SetHovered(card, false);
    }

    #endregion

    #region 누름 (드래그 시작 전 - 놓을 수 있는 칸 미리 표시)

    private void Press(HandCardView card, DragSource source, PointerEventData eventData)
    {
        
        if (_dragCard != null || _pressedCard != null) return; // 드래그 중/다른 손가락으로 누르는 중이면 무시
        if (!IsLeftButton(eventData)) return;
        if (!TryGetDragOrigin(card, source, out int fromSlot)) return;

        _pressedCard = card;
        _pressPointerId = eventData != null ? eventData.pointerId : 0;
        ShowAvailableSlots(fromSlot);
    }

    // 드래그 없이 뗌(제자리 클릭)
    private void Release(HandCardView card, PointerEventData eventData)
    {
        if (_pressedCard != card) return;
        if (eventData != null && eventData.pointerId != _pressPointerId) return;
        ClearPress();
        if (_dragCard == null) _fieldBoardView.ClearSlotHighlights();
    }

    private void ClearPress()
    {
        _pressedCard = null;
        _pressPointerId = 0;
    }

    // 놓을 수 있는 칸 = 원래 칸을 뺀 전부(빈 칸 = 배치/이동, 카드 있는 칸 = 교체/교환).
    private void ShowAvailableSlots(int fromSlot)
    {
    
        for (int i = 0; i < _fieldBoardView.SlotCount; i++)
            _fieldBoardView.SetSlotAvailable(i, i != fromSlot && _placementController.IsValidSlot(i));
    }

    #endregion

    #region 드래그

    private void BeginDrag(HandCardView card, DragSource source, PointerEventData eventData)
    {
        if (_dragCard != null) return; // 한 번에 1장(멀티터치/중복 드래그 방지)
        if (!IsLeftButton(eventData)) return;
        if (!TryGetDragOrigin(card, source, out int fromSlot)) return;

        ClearHover();
        if (source == DragSource.Hand)
        {
            if (!_handLayoutView.BeginDragOut(card)) return;
        }
        else
        {
            _fieldBoardView.BeginDragOut(fromSlot);
        }

        ClearPress();
        ShowAvailableSlots(fromSlot); // 누름(Down)을 못 받은 경우 대비 - 이미 켜져 있으면 변화 없음
        _dragCard = card;
        _dragSource = source;
        _dragFromSlot = fromSlot;
        _dragPointerId = eventData != null ? eventData.pointerId : 0;

        Vector2 screenPos = eventData != null ? eventData.position : Vector2.zero;
        _dragView.BeginDrag(card, screenPos);
        UpdateDropHighlight(screenPos);
    }

    private void UpdateDrag(PointerEventData eventData)
    {
        _dragView.Drag(eventData.position);
        UpdateDropHighlight(eventData.position);
    }

    // 놓는 순간 1번만 판정, 연출. 실패/무효면 원래 자리로.
    private void EndDrag(PointerEventData eventData)
    {
        HandCardView card = _dragCard;
        DragSource source = _dragSource;
        int fromSlot = _dragFromSlot;
        DropTarget target = ResolveTarget(eventData.position, out int slotIndex, out Vector3 worldPos);

        ResetDragState();
        _dragView.EndDrag();

        bool committed = source == DragSource.Hand
            ? CommitFromHand(card, target, slotIndex)
            : CommitFromField(card, fromSlot, target, slotIndex, worldPos);

        _fieldBoardView.ClearSlotHighlights(); // 안착한 칸은 AttachCard가 강조를 넘겨받아 번쩍임으로 이어감
        if (!committed) Revert(card, source, fromSlot);
        WarnIfOutOfSync();
    }

    // 드래그 강제 종료(포커스 상실/페이즈 종료) - 상태 변경 없이 원래 자리로.
    private void CancelActiveDrag()
    {
        if (_dragCard == null) return;
        HandCardView card = _dragCard;
        DragSource source = _dragSource;
        int fromSlot = _dragFromSlot;

        ResetDragState();
        _dragView.EndDrag();
        _fieldBoardView.ClearSlotHighlights();
        Revert(card, source, fromSlot);
    }

    private void ResetDragState()
    {
        _dragCard = null;
        _dragSource = DragSource.None;
        _dragFromSlot = -1;
        _dragPointerId = 0;
    }

    private void Revert(HandCardView card, DragSource source, int fromSlot)
    {
        if (source == DragSource.Hand) _handLayoutView.CancelDragOut(card);
        else if (source == DragSource.Field) _fieldBoardView.CancelDragOut(fromSlot);
    }

    private void UpdateDropHighlight(Vector2 screenPos)
    {
        DropTarget target = ResolveTarget(screenPos, out int slotIndex, out _);
        bool highlight = target == DropTarget.Slot && slotIndex != _dragFromSlot; // 원래 칸 위는 강조 안 함(이동 없음)
        _fieldBoardView.SetTargetSlot(highlight ? slotIndex : -1);
    }

    // 이 카드를 집을 수 있는지(View 목록과 상태 원본이 둘 다 같은 위치로 알고 있을 때만) + 출발 칸(손패면 -1).
    private bool TryGetDragOrigin(HandCardView card, DragSource source, out int fromSlot)
    {
        fromSlot = -1;
        CardRuntime runtime = card != null ? card.Runtime : null;
        if (runtime == null) return false;

        if (source == DragSource.Hand)
            return _hand.IndexOf(runtime) >= 0 && _handLayoutView.IndexOf(card) >= 0;

        fromSlot = _fieldBoardView.IndexOf(card);
        return fromSlot >= 0 && _placementController.GetSlot(fromSlot) == runtime;
    }

    private static bool IsLeftButton(PointerEventData eventData)
    {
        return eventData == null || eventData.button == PointerEventData.InputButton.Left; // 터치도 Left로 들어옴
    }

    // 포인터 위치 -> 놓을 곳. 패널이 손패 영역과 겹치면 패널 우선. 화면 밖 = 무효.
    private DropTarget ResolveTarget(Vector2 screenPos, out int slotIndex, out Vector3 worldPos)
    {
        slotIndex = -1;
        if (!_dragView.TryGetPointerWorld(screenPos, out worldPos)) return DropTarget.None;
        if (_fieldBoardView.TryGetSlotAt(worldPos, out slotIndex)) return DropTarget.Slot;
        if (_handLayoutView.IsInDropArea(worldPos)) return DropTarget.Hand;
        return DropTarget.None;
    }

    #endregion

    #region 확정

    // 손패에서 집은 카드: 패널 위 = 배치/교체, 그 외(손패 영역 포함) = 원래 자리.
    private bool CommitFromHand(HandCardView card, DropTarget target, int slotIndex)
    {
        if (target != DropTarget.Slot) return false;

        PlacementResultData result = _placementController.PlaceFromHand(card.Runtime, slotIndex);
        if (!result.IsSuccess) { LogReject(result); return false; }

        if (result.Kind == PlacementActionKind.Replaced)
        {
            // 패널에 있던 카드 -> 드래그한 카드가 있던 손패 자리로
            HandCardView displaced = _fieldBoardView.DetachCard(slotIndex);
            _handLayoutView.TakeCard(card);
            if (displaced != null) _handLayoutView.InsertCard(result.HandIndex, displaced);
        }
        else
        {
            _handLayoutView.TakeCard(card);
        }

        _fieldBoardView.AttachCard(slotIndex, card);
        return true;
    }

    // 패널에서 집은 카드: 다른 패널 = 이동/교환, 손패 영역 = 회수, 그 외 = 원래 자리.
    private bool CommitFromField(HandCardView card, int fromSlot, DropTarget target, int slotIndex, Vector3 worldPos)
    {
        if (target == DropTarget.Slot)
        {
            if (slotIndex == fromSlot) return false; // 같은 칸 - 변화 없음

            PlacementResultData result = _placementController.MoveBetweenSlots(fromSlot, slotIndex);
            if (!result.IsSuccess) { LogReject(result); return false; }

            HandCardView moving = _fieldBoardView.DetachCard(fromSlot);
            HandCardView other = result.Kind == PlacementActionKind.Swapped ? _fieldBoardView.DetachCard(slotIndex) : null;
            _fieldBoardView.AttachCard(slotIndex, moving != null ? moving : card);
            if (other != null) _fieldBoardView.AttachCard(fromSlot, other);
            return true;
        }

        if (target == DropTarget.Hand)
        {
            int insertIndex = _handLayoutView.GetDropInsertIndex(worldPos);
            PlacementResultData result = _placementController.ReturnToHand(fromSlot, insertIndex);
            if (!result.IsSuccess) { LogReject(result); return false; }

            HandCardView returning = _fieldBoardView.DetachCard(fromSlot);
            _handLayoutView.InsertCard(result.HandIndex, returning != null ? returning : card);
            return true;
        }

        return false;
    }

    private static void LogReject(PlacementResultData result)
    {
        // 같은 칸에 다시 놓기는 정상 조작 - 그 외는 상태 불일치 가능성이라 경고
        if (result.RejectReason == PlacementRejectReason.SameSlot) return;
        Debug.LogWarning($"[CardPlacementOrchestrator] 배치 거부: {result.RejectReason}");
    }

    // View 목록과 상태 원본 장수 비교
    private void WarnIfOutOfSync()
    {
        if (_handLayoutView.Count != _hand.Count)
            Debug.LogWarning($"[CardPlacementOrchestrator] 손패 불일치 - View {_handLayoutView.Count}장 / 로직 {_hand.Count}장");
    }

    #endregion
}