using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "HanddrawConfig", menuName = "Battle/Hand draw Config")]
public class HanddrawConfig : ScriptableObject
{
    [Header("드로우 매수")]
    [Min(1)] public int firstTurnDrawCount = 6;
    [Min(1)] public int perTurnDrawCount = 2;

    [Header("부채꼴 배치")]
    [Min(0f)] public float fanRadius = 14f;
    [Min(0f)] public float maxFanAngle = 18f;
    [Min(0f)] public float maxCardStepAngle = 4f;

    [Header("연속 드로우")]
    [FormerlySerializedAs("drawStagger")]
    [Min(0f)] public float drawInterval = 0.12f;

    [Header("정렬")]
    public string pullOutSortingLayer = "Board";
    public int pullOutSortingOrder = -15;
    public string handSortingLayer = "Hand";
    public int handSortingOrderBase = 100;

    [Header("1단계 : 덱에서 뽑아내기 (덱 밑에서 옆으로 빠져나옴")]
    public Vector2 pullOutOffset = new Vector2(-160f, 0f);
    public bool autoPullOutDistance = true;
    [Min(0f)] public float pullOutMargin = 0.2f;
    [Min(0f)] public float pullOutDuration = 0.15f;

    [Header("2단계 : 손패로 이동")]
    [Min(0f)] public float drawMoveDuration = 0.35f;
    public AnimationCurve drawMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Range(0f, 1f)] public float makeRoomStartRatio = 0.4f;

    [Header("카드 뒤집기 (2단계 이동 도중 재생")]
    [Min(0f)] public float flipDuration = 0.25f;
    [Range(0f, 1f)] public float flipStartRatio = 0.2f;

    [Header("리셔플 연출")]
    [Min(0f)] public float reshuffleDuration = 0.3f;
}
