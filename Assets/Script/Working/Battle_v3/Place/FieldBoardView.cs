using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

// 고정 카드 패널(기본 3칸)과 거기 놓인 카드 View의 연출만 담당
public class FieldBoardView : MonoBehaviour
{
    [SerializeField] private FieldSlotView[] slots;  // 왼쪽부터 순서대로(칸 수 = CardPlacementController 칸 수)
    [SerializeField] private PlacementConfig config;

    // 상향식 보고 - 놓인 카드의 포인터 입력 중계
    public event Action<HandCardView, CardPointerEventKind, PointerEventData> OnCardPointer;

    private HandCardView[] _cards;
    private int[] _slotVersions;      // 칸별 연출 세대 - 안착 이동 도중 그 칸이 바뀌면(드래그/교체/회수) 뒤늦은 튕김·강조를 건너뜀
    private int _dragOutSlot = -1;    // 카드를 집어 든 칸(확정 전이라 _cards에는 남아 있음)
    private bool[] _available;        // 칸별 "놓을 수 있음" 강조 여부(카드를 누르거나 잡은 동안 - Orchestrator가 지정)
    private int _targetSlot = -1;     // 마우스가 올라간 대상 칸(가장 강한 강조)
    private int _fieldLayerId;
    private Action<CardPointerView, CardPointerEventKind, PointerEventData> _pointerRelay; // 구독용 델리게이트 1회 캐싱

    // 공용 깜빡임
    private float _pulseValue;
    private Tween _pulseTween;
    private DOGetter<float> _pulseGetter;
    private DOSetter<float> _pulseSetter;

    public int SlotCount => slots != null ? slots.Length : 0;

    private void Awake()
    {
        _cards = new HandCardView[SlotCount];
        _slotVersions = new int[SlotCount];
        _available = new bool[SlotCount];
        _fieldLayerId = SortingLayerUtility.ResolveId(config != null ? config.fieldSortingLayer : null, nameof(FieldBoardView));
        _pointerRelay = RelayPointer;
        _pulseGetter = () => _pulseValue;
        _pulseSetter = v => { _pulseValue = v; PushPulseToSlots(); };
    }

    private void OnDestroy()
    {
        _pulseTween?.Kill();
    }

    public bool IsValidSlot(int slotIndex) => slotIndex >= 0 && slotIndex < SlotCount && slots[slotIndex] != null;

    public HandCardView GetCard(int slotIndex) => IsValidSlot(slotIndex) ? _cards[slotIndex] : null;

    public int IndexOf(HandCardView cardView)
    {
        if (cardView == null || _cards == null) return -1;
        for (int i = 0; i < _cards.Length; i++)
        {
            if (_cards[i] == cardView) return i;
        }
        return -1;
    }

    // 월드 좌표 아래 패널 번호(패널 이미지 영역 기준).
    public bool TryGetSlotAt(Vector3 worldPos, out int slotIndex)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (slots[i] != null && slots[i].Contains(worldPos)) { slotIndex = i; return true; }
        }
        slotIndex = -1;
        return false;
    }

    #region 패널 테두리 강조 (가능 / 대상) - 어느 칸이 가능한지는 Orchestrator가 판정해서 지정

    // 놓을 수 있는 칸 표시 켜기/끄기(카드를 누르는 순간 ~ 놓을 때까지)
    public void SetSlotAvailable(int slotIndex, bool available)
    {
        if (!IsValidSlot(slotIndex) || _available[slotIndex] == available) return;
        _available[slotIndex] = available;
        UpdatePulseRunning();  
        RefreshSlotHighlight(slotIndex);
    }

    // 마우스가 올라간 대상 칸(-1 = 없음)
    public void SetTargetSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) slotIndex = -1;
        if (_targetSlot == slotIndex) return;

        int previous = _targetSlot;
        _targetSlot = slotIndex;
        if (DimOthersOnTarget)
        {
            // 대상이 생기거나 사라지면 나머지 칸도 꺼짐
            for (int i = 0; i < SlotCount; i++) RefreshSlotHighlight(i);
            return;
        }
        RefreshSlotHighlight(previous);   // 이전 대상 -> 가능(깜빡임)으로 복귀 또는 꺼짐
        RefreshSlotHighlight(slotIndex);
    }

    // 전부 끄기(놓기/취소/페이즈 종료)
    public void ClearSlotHighlights()
    {
        int previousTarget = _targetSlot;
        _targetSlot = -1;
        for (int i = 0; i < SlotCount; i++)
        {
            if (!_available[i] && i != previousTarget) continue;
            _available[i] = false;
            RefreshSlotHighlight(i);
        }
        UpdatePulseRunning();
    }

    // 칸 1개를 현재 단계에 맞게
    private void RefreshSlotHighlight(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return;
        FieldSlotView slot = slots[slotIndex];
        float fade = HighlightFade;

        if (slotIndex == _targetSlot)
        {
            slot.ShowTarget(TargetColor, config != null ? config.slotDropHighlightAlpha : 1f, fade);
        }
        else if (_available[slotIndex] && !(DimOthersOnTarget && _targetSlot >= 0))
        {
            slot.SetPulseAlpha(_pulseValue); // 합류 시점의 공용 값부터 시작
            slot.ShowAvailable(config != null ? config.slotAvailableColor : Color.white, fade);
        }
        else
        {
            slot.Hide(fade); // 꺼짐 / 대상 칸이 있는 동안의 나머지 칸
        }
    }

    //공용 깜빡임 실행, 없으면 정지. 새로 시작할 때는 항상 최소값(꺼짐)부터 -> 매번 같은 박자.
    private void UpdatePulseRunning()
    {
        bool anyAvailable = false;
        for (int i = 0; i < SlotCount; i++)
        {
            if (_available[i]) { anyAvailable = true; break; }
        }

        if (!anyAvailable)
        {
            _pulseTween?.Kill();
            _pulseTween = null;
            return;
        }
        if (_pulseTween != null && _pulseTween.IsActive()) return; 

        float min = config != null ? config.slotAvailableAlphaMin : 0f;
        float max = config != null ? config.slotAvailableAlphaMax : 1f;
        float half = config != null ? config.slotAvailablePulseDuration : 0.5f;
        if (half <= 0f) { _pulseValue = max; PushPulseToSlots(); return; } 

        _pulseValue = min;
        _pulseTween = DOTween.To(_pulseGetter, _pulseSetter, max, half)
                             .SetEase(Ease.InOutSine)
                             .SetLoops(-1, LoopType.Yoyo) 
                             .SetLink(gameObject);
    }

    // 공용 값을 모든 칸에 전달
    private void PushPulseToSlots()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (slots[i] != null) slots[i].SetPulseAlpha(_pulseValue);
        }
    }

    private bool DimOthersOnTarget => config == null || config.dimOtherSlotsOnTarget;

    private bool IsSlotHighlighted(int slotIndex) => _available[slotIndex] || _targetSlot == slotIndex;

    #endregion

    // 카드 받기(손패에서 / 다른 칸에서) -> 칸 중앙으로 이동 + 패널 크기로 맞춤 -> 안착 튕김 + 패널 짧은 강조. 연출은 기다리지 않음.
    public void AttachCard(int slotIndex, HandCardView cardView)
    {
        if (!IsValidSlot(slotIndex) || cardView == null) return; // 예외 방지
        if (_cards[slotIndex] != null && _cards[slotIndex] != cardView)
        {
            Debug.LogWarning($"[FieldBoardView] {slotIndex}번 패널에 이미 카드가 있습니다 - DetachCard 후 호출해야 합니다.");
            return;
        }

        _cards[slotIndex] = cardView;
        Subscribe(cardView);
        if (_dragOutSlot == slotIndex) _dragOutSlot = -1;
        if (_targetSlot == slotIndex) _targetSlot = -1;
        _available[slotIndex] = false;
        UpdatePulseRunning();

        cardView.ReparentTo(slots[slotIndex].CardAnchor);
        ApplyFieldSorting(cardView);
        int version = ++_slotVersions[slotIndex];
        float duration = config != null ? config.dropMoveDuration : 0.18f;
        PlaySettleAsync(slotIndex, cardView, version, playFlash: true, duration, destroyCancellationToken).Forget();
    }

    // 카드 넘기기(다른 칸/손패로) - 구독 해제 후 반환. 부모 교체는 받는 쪽이 함.
    public HandCardView DetachCard(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return null;
        HandCardView cardView = _cards[slotIndex];
        if (cardView == null) return null;

        Unsubscribe(cardView);
        _cards[slotIndex] = null;
        _slotVersions[slotIndex]++;
        if (_dragOutSlot == slotIndex) _dragOutSlot = -1;
        return cardView;
    }

    // 놓인 카드를 집어 듦(확정 전)
    public void BeginDragOut(int slotIndex)
    {
        if (!IsValidSlot(slotIndex) || _cards[slotIndex] == null) return;
        _dragOutSlot = slotIndex;
        _slotVersions[slotIndex]++;
    }

    // 드래그 취소(무효 위치/규칙 거부) - 원래 칸 중앙으로 복귀(강조 없음).
    public void CancelDragOut(int slotIndex)
    {
        if (_dragOutSlot == slotIndex) _dragOutSlot = -1;
        HandCardView cardView = GetCard(slotIndex);
        if (cardView == null) return;

        ApplyFieldSorting(cardView);
        int version = ++_slotVersions[slotIndex];
        float duration = config != null ? config.returnDuration : 0.2f;
        PlaySettleAsync(slotIndex, cardView, version, playFlash: false, duration, destroyCancellationToken).Forget();
    }

    // 전부 풀 반환(라운드 리셋/테스트 재초기화)
    public void ClearAll()
    {
        if (_cards == null) return;
        for (int i = 0; i < _cards.Length; i++)
        {
            HandCardView cardView = _cards[i];
            _cards[i] = null;
            _slotVersions[i]++;
            _available[i] = false;
            if (slots[i] != null) slots[i].Hide(0f);
            if (cardView == null) continue;

            Unsubscribe(cardView);
            if (WorldPoolManager.instance != null) WorldPoolManager.instance.Return(cardView.gameObject);
        }
        _dragOutSlot = -1;
        _targetSlot = -1;
        UpdatePulseRunning();
    }

    private async UniTaskVoid PlaySettleAsync(int slotIndex, HandCardView cardView, int version, bool playFlash, float duration, CancellationToken ct)
    {
        float fitScale = GetFitScale(slotIndex, cardView);
        Ease ease = config != null ? config.dropMoveEase : Ease.OutCubic;
        await cardView.MoveToAsync(Vector2.zero, 0f, fitScale, duration, ease, ct);

        // 도중에 칸이 바뀌었거나(다시 집음/교체) 번쩍임 없는 복귀 - 넘겨받았던 테두리를 현재 단계로 되돌림
        if (version != _slotVersions[slotIndex] || _cards[slotIndex] != cardView || !playFlash)
        {
            RefreshSlotHighlight(slotIndex);
            return;
        }
        
        if (IsSlotHighlighted(slotIndex)) return;

        float punch = config != null ? config.placePunchScale : 0.08f;
        float punchDuration = config != null ? config.placePunchDuration : 0.12f;
        float flashDuration = config != null ? config.placeFlashDuration : 0.25f;
        float peak = config != null ? config.slotDropHighlightAlpha : 1f;
        await UniTask.WhenAll(cardView.PlayPunchAsync(punch, punchDuration, ct),
                              slots[slotIndex].PlayFlashAsync(TargetColor, peak, flashDuration, ct));
    }

    // 패널 크기 × fieldFitRatio 안에 들어가는 배율(가로/세로 중 작은 쪽). 계산 불가면 1.
    private float GetFitScale(int slotIndex, HandCardView cardView)
    {
        Vector2 slotSize = slots[slotIndex].GetAnchorLocalSize();
        Vector2 cardSize = cardView.GetLocalSizeAtScale(1f);
        if (slotSize.x <= 0f || slotSize.y <= 0f || cardSize.x <= 0f || cardSize.y <= 0f) return 1f; 

        float ratio = config != null ? config.fieldFitRatio : 0.9f;
        return Mathf.Min(slotSize.x / cardSize.x, slotSize.y / cardSize.y) * ratio;
    }

    // 놓인 카드는 한 칸에 1장이라 서로 안 겹침 -> 같은 order, 입력 깊이 0.
    private void ApplyFieldSorting(HandCardView cardView)
    {
        cardView.SetSorting(_fieldLayerId, config != null ? config.fieldCardOrder : 10);
        cardView.SetRaycastDepth(0f);
    }

    private Color TargetColor => config != null ? config.slotTargetColor : Color.white;
    private float HighlightFade => config != null ? config.slotHighlightFadeDuration : 0.08f;

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
}