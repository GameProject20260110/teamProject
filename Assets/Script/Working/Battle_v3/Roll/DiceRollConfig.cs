using UnityEngine;

[CreateAssetMenu(fileName = "DiceRollConfig", menuName = "Battle/Dice Roll Config")]
public class DiceRollConfig : ScriptableObject
{
    [Header("제목")]
    public string title = "기본 공격력 걸졍";

    [Header("정렬 (팝업 공용 레이어)")]
    public string sortingLayer = "Popup";
    public int panelOrder = 1;
    public int backgroundOrder = 0;
    public int titleOrder = 1;
    public int itemBaseOrder = 10;

    [Header("오버레이 / 패널 등장")]
    [Range(0f, 1f)] public float overlayAlpha = 0.65f;
    [Min(0f)] public float overlayFadeDuration = 0.25f;
    [Range(0f, 1f)] public float panelAlpha = 1f;
    [Min(0f)] public float panelFadeDuration = 0.2f;

    [Header("패널 안 영역 (월드 유닛)")]
    public Vector2 panelPadding = new Vector2(0.4f, 0.3f);
    [Min(0f)] public float titleAreaHeight = 0.8f;
    
    [Header("항목 배치")]
    public Vector2 itemSize = new Vector2(2f, 4f);
    [Min(0f)] public float itemGap = 0.5f;
    [Min(0f)] public float rowGap = 0.3f;
    [Range(0.1f, 1f)] public float minItemScale = 0.6f;
    [Min(1f)] public int maxRows = 2;

    [Header("항목 순차 등장")]
    [Min(0f)] public float itemAppearDuration = 0.2f;
    [Min(0f)] public float itemAppearStagger = 0.02f;

    [Header("굴림")]
    [Min(0f)] public float rollDuration = 0.35f;
    [Min(0.01f)] public float faceSwapInterval = 0.05f;

    [Header("순차 정지 - 왼쪽부터 하나씩")]
    [Min(0f)] public float stopInterval = 0.06f;
    [Min(0f)] public float maxStopSpread = 0.4f;
    [Min(1f)] public float stopPunchScale = 1.1f;
    [Min(0f)] public float stopPunchDuration = 0.1f;
    [Min(0f)] public float revealDuration = 0.15f;
    [Range(0f, 1f)] public float revealStartScale = 0.6f;
    [Min(0f)] public float revealOvershoot = 1.2f;

    [Header("스킵")]
    [Min(0f)] public float skipInputDelay = 0.25f;

    [Header("종료")]
    [Min(0f)] public float closeHoldDelay = 0.3f;
    [Min(0f)] public float closeFadeDuration = 0.15f;
}
