using UnityEngine;

[CreateAssetMenu(fileName = "MulliganConfig", menuName = "Battle/Mulligan Config")]
public class MulliganConfig : ScriptableObject
{
    [Header("정렬 (Hand보다 위에 그려지는 Sorting Layer")]
    public string sortingLayer = "Popup";
    public int cardBaseOrder = 10;

    [Header("오버레이 / 패널")]
    [Range(0f, 1f)] public float overlayAlpha = 0.65f;
    [Min(0f)] public float showFadeDuration = 0.25f;

    [Header("패널 카드 배치")]
    [Min(0f)] public float cardScale = 1f;
    [Min(0f)] public float cardSpacing = 1.6f;
    [Min(0f)] public float maxRowWidth = 12;

    [Header("패널 열림")]
    [Min(0f)] public float cardAppearDuration = 0.3f;
    [Min(0f)] public float cardAppearStagger = 0.03f;
    [Min(0f)] public float confirmButtonFadeDuraiton = 0.2f;

    [Header("카드 선택 강조")]
    [Min(0f)] public float selectRaise = 0.3f;
    [Min(0f)] public float selectDuration = 0.12f;
    [Range(0f, 1f)] public float selectBorderAlpha = 1f;
    [Range(0f, 1f)] public float selectGlowAlpha = 0.35f;

    [Header("버튼 클릭 후 - 카드 교체")]
    [Min(0f)] public float exitDuration = 0.12f;
    [Min(0f)] public float enterDuration = 0.15f;

    [Header("버튼 클릭 후 - 패널 자동 닫힘")]
    [Range(0f, 0.5f)] public float closeHoldDelay = 0.4f;
    [Min(0f)] public float closeFadeDuration = 0.25f;
}
