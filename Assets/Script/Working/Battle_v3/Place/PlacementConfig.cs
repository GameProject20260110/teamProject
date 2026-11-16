using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "PlacementConfig", menuName = "Battle/Placement Config")]
public class PlacementConfig : ScriptableObject
{
    [Header("손패 Hover (위로 살짝 + 확대 + 맨 앞 + 나머지 벌어짐 - 카드 테두리 강조 없음)")]
    [Min(0f)] public float hoverRisePixels = 12f;       
    [Min(0f)] public float hoverScale = 1.1f;
    [Min(0f)] public float hoverDuration = 0.1f;
    public int hoverOrderBoost = 50;                    
    [Min(0f)] public float hoverSpread = 0.4f;
    [Range(0f, 1f)] public float hoverSpreadFalloff = 0.5f; 

    [Header("손패 재정렬 (카드가 빠지거나 들어올 때)")]
    [Min(0f)] public float handRelayoutDuration = 0.18f;
    [Min(0f)] public float raycastDepthPerOrder = 0.001f;

    [Header("손패 드롭 영역 (handContainer 로컬 - 놓인 카드를 손패로 회수하는 범위)")]
    public Vector2 handDropAreaCenter = new Vector2(0f, 0f);
    public Vector2 handDropAreaSize = new Vector2(14f, 4f);

    [Header("드래그 (마우스 따라감 + 모든 요소 위 + 약간 확대)")]
    public string dragSortingLayer = "Drag";
    public int dragSortingOrder = 0;
    [Min(0f)] public float dragScale = 1.15f;           
    [Min(0f)] public float dragPoseDuration = 0.08f;    

    [Header("패널 (필드)")]
    public string fieldSortingLayer = "Field";
    public int fieldCardOrder = 10;
    [Range(0.1f, 1f)] public float fieldFitRatio = 0.9f;       

    [Header("패널 테두리 강조 (Hover = 카드만 / 카드를 누르거나 잡으면 = 놓을 수 있는 칸 전부)")]
    public Color slotAvailableColor = new Color(1f, 0.84f, 0.35f, 1f); 
    [Range(0f, 1f)] public float slotAvailableAlphaMax = 0.8f;          
    [Range(0f, 1f)] public float slotAvailableAlphaMin = 0f;
    [Min(0f)] public float slotAvailablePulseDuration = 0.5f;
    public bool dimOtherSlotsOnTarget = true;
    public Color slotTargetColor = new Color(1f, 0.95f, 0.7f, 1f);     
    [Range(0f, 1f)] public float slotDropHighlightAlpha = 1f;           
    [Min(0f)] public float slotHighlightFadeDuration = 0.1f;           

    [Header("드롭 → 패널 안착")]
    [Min(0f)] public float dropMoveDuration = 0.18f;    
    public Ease dropMoveEase = Ease.OutCubic;
    [Min(0f)] public float placePunchScale = 0.08f;     
    [Min(0f)] public float placePunchDuration = 0.12f;
    [Min(0f)] public float placeFlashDuration = 0.25f;  

    [Header("복귀 (손패로 회수 / 무효 위치 → 원래 자리)")]
    [Min(0f)] public float returnDuration = 0.2f;
}