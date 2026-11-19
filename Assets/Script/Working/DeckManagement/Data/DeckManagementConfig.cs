using UnityEngine;

// 덱 관리 시스템에서 쓰는 수치 전부를 모아둔 설정 So
[CreateAssetMenu(fileName = "DeckManagementConfig", menuName = "Deck/DeckManagementConfig")]
public class DeckManagementConfig : ScriptableObject
{
    [Header("덱 제한")]
    [Min(1)] public int maxCardsPerDeck = 20;
    [Min(1)] public int maxSavedDecks = 5;

    [Header("기본 값")]
    public string defaultDeckName = "새 덱";

    [Header("영역별 카드 크기")]
    public Vector2 deckCardSize = new Vector2(142f, 213f);
    public Vector2 ownedCardSize = new Vector2(99f, 148f);

    [Header("연출 시간/속도")]
    [Min(0.01f)] public float dragSizeLerpSpeed = 12f;
    [Min(0.01f)] public float returnDuration = 0.3f;
    [Min(0.01f)] public float quickMoveDuration = 0.35f;
    [Min(0.01f)] public float toastVisibleDuration = 1.2f;
    [Min(0.01f)] public float toastFadeDuration = 0.25f;
    [Min(0.01f)] public float sceneFadeDuration = 0.25f;
    [Min(0.01f)] public float panelSlideDuration = 0.25f;
    [Min(0.01f)] public float SelectScaleDuration = 0.15f;

    [Header("호버 연출")]
    [Min(1f)] public float hoverScale = 1.08f;
}
