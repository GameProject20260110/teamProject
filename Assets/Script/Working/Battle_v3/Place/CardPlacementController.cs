using System;
using System.Transactions;
using UnityEngine;

public class CardPlacementController
{
    private readonly HandData _hand;
    private readonly CardRuntime[] _slots;

    public int SlotCount => _slots.Length;

    // 배치가 실제로 바뀐 뒤 1회
    public event Action<PlacementResultData> OnPlacementChanged;

    public CardPlacementController(HandData hand, int slotCount)
    {
        _hand = hand;
        _slots = new CardRuntime[Math.Max(0, slotCount)];
    }

    public bool IsValidSlot(int slotIndex) => slotIndex >= 0 && slotIndex < _slots.Length;

    public CardRuntime GetSlot(int slotIndex) => IsValidSlot(slotIndex) ? _slots[slotIndex] : null;

    public int IndexOfSlot(CardRuntime card)
    {
        if (card == null) return -1;
        for(int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == card) return i;
        }
        return -1;
    }

    public int PlacedCount
    {
        get
        {
            int count = 0;
            for(int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != null) count++;
            }
            return count;
        }
    }

    public PlacementResultData PlaceFromHand(CardRuntime card, int slotIndex)
    {
        if(card == null || _hand == null) return PlacementResultData.Reject(PlacementRejectReason.InvalidCard);
        if(!IsValidSlot(slotIndex)) return PlacementResultData.Reject(PlacementRejectReason.InvalidSlot);
        if(IndexOfSlot(card) >= 0) return PlacementResultData.Reject(PlacementRejectReason.AlreadyPlaced);

        int handIndex = _hand.IndexOf(card);
        if (handIndex < 0) return PlacementResultData.Reject(PlacementRejectReason.NotInHand);

        // TODO(코스트) : 남은 코스트 < card.data.cost 이면 Reject(NotEnoughCost). 교체면 빠지는 카드 코스트 환급 후 비교

        CardRuntime existing = _slots[slotIndex];
        if(existing == null)
        {
            if(!_hand.RemoveAt(handIndex)) return PlacementResultData.Reject(PlacementRejectReason.NotInHand);
            _slots[slotIndex] = card;
            return Commit(new PlacementResultData(PlacementActionKind.Placed, card, null, PlacementResultData.HandSlot, slotIndex, handIndex));
        }
        // 교체 : 패널에 있던 카드가 드래그한 카드의 손패 자리로 들어감
        if (!_hand.SetAt(handIndex, existing)) return PlacementResultData.Reject(PlacementRejectReason.NotInHand);
        _slots[slotIndex] = card;
        return Commit(new PlacementResultData(PlacementActionKind.Replaced, card, existing, PlacementResultData.HandSlot, slotIndex, handIndex));

        
    }

    // 패널 -> 패널(빈 칸 = 이동 / 카드 있는 칸 = 교환)
    public PlacementResultData MoveBetweenSlots(int fromSlot, int toSlot)
    {
        if(!IsValidSlot(fromSlot) || !IsValidSlot(toSlot)) return PlacementResultData.Reject(PlacementRejectReason.InvalidSlot);
        if(fromSlot == toSlot) return PlacementResultData.Reject(PlacementRejectReason.SameSlot);

        CardRuntime moving = _slots[fromSlot];
        if(moving == null) return PlacementResultData.Reject(PlacementRejectReason.EmptySlot);

        CardRuntime other = _slots[toSlot];
        _slots[toSlot] = moving;
        _slots[fromSlot] = other;

        PlacementActionKind kind = other == null ? PlacementActionKind.Moved : PlacementActionKind.Swapped;
        return Commit(new PlacementResultData(kind, moving, other, fromSlot, toSlot, -1));
    }

    // 패널 -> 손패
    public PlacementResultData ReturnToHand(int fromSlot, int HandInsertIndex)
    {
        if(_hand == null) return PlacementResultData.Reject(PlacementRejectReason.InvalidCard);
        if(!IsValidSlot(fromSlot)) return PlacementResultData.Reject(PlacementRejectReason.InvalidSlot);

        CardRuntime card = _slots[fromSlot];
        if(card == null) return PlacementResultData.Reject(PlacementRejectReason.EmptySlot);

        // TODO(코스트) : 회수 시 코스트 환급

        int insertedIndex = _hand.Insert(HandInsertIndex, card);
        if (insertedIndex < 0) return PlacementResultData.Reject(PlacementRejectReason.InvalidCard);

        _slots[fromSlot] = null;
        return Commit(new PlacementResultData(PlacementActionKind.Returned, card, null, fromSlot, PlacementResultData.HandSlot, insertedIndex));
    }

    private PlacementResultData Commit(PlacementResultData result)
    {
        OnPlacementChanged?.Invoke(result);
        return result;
    }
}
