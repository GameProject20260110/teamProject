// 카드 배치 판정 결과(순수 데이터, MonoBehaviour 아님). CardPlacementController가 만들고 Orchestrator가 읽어서 연출을 고른다.

public enum PlacementActionKind
{
    Rejected,   // 규칙 위반 - 아무것도 안 바뀜(드래그한 카드는 원래 자리로)
    Placed,     // 손패 → 빈 패널
    Replaced,   // 손패 → 카드 있는 패널(기존 카드는 드래그한 카드가 있던 손패 자리로)
    Moved,      // 패널 → 빈 패널
    Swapped,    // 패널 → 카드 있는 패널(두 카드 자리 교환)
    Returned,   // 패널 → 손패
}

public enum PlacementRejectReason
{
    None,
    InvalidCard,    // null 카드 / Controller 미초기화
    InvalidSlot,    // 범위 밖 패널 번호
    SameSlot,       // 같은 패널에 다시 놓음(이동 없음)
    NotInHand,      // 손패에 없는 카드
    AlreadyPlaced,  // 이미 다른 패널에 놓인 카드(중복 배치 금지)
    EmptySlot,      // 출발 패널이 비어 있음
    // TODO(코스트): NotEnoughCost - 코스트 시스템 추가 시
}

public readonly struct PlacementResultData
{
    public const int HandSlot = -1; // FromSlot/ToSlot 값 - 손패

    public readonly PlacementActionKind Kind;
    public readonly PlacementRejectReason RejectReason;
    public readonly CardRuntime Card;       // 드래그한 카드
    public readonly CardRuntime OtherCard;  // 교체로 손패에 간 카드 / 교환된 카드(없으면 null)
    public readonly int FromSlot;           // 출발 패널(HandSlot = 손패)
    public readonly int ToSlot;             // 도착 패널(HandSlot = 손패)
    public readonly int HandIndex;          // 손패 쪽 목록 인덱스(출발 자리 / 교체 카드가 들어간 자리 / 회수돼 들어간 자리), 없으면 -1

    public bool IsSuccess => Kind != PlacementActionKind.Rejected;

    public PlacementResultData(PlacementActionKind kind, CardRuntime card, CardRuntime otherCard, int fromSlot, int toSlot, int handIndex)
    {
        Kind = kind;
        RejectReason = PlacementRejectReason.None;
        Card = card;
        OtherCard = otherCard;
        FromSlot = fromSlot;
        ToSlot = toSlot;
        HandIndex = handIndex;
    }

    private PlacementResultData(PlacementRejectReason reason)
    {
        Kind = PlacementActionKind.Rejected;
        RejectReason = reason;
        Card = null;
        OtherCard = null;
        FromSlot = HandSlot;
        ToSlot = HandSlot;
        HandIndex = -1;
    }

    public static PlacementResultData Reject(PlacementRejectReason reason) => new PlacementResultData(reason);
}