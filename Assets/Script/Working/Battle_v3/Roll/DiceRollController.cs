using UnityEngine;
using System.Collections.Generic;

public class DiceRollController
{
    private readonly HandData _hand;
    private readonly System.Random _random;
    private readonly HashSet<CardData> _warnedData = new HashSet<CardData>();

    public DiceRollController(HandData hand, int? seed = null)
    {
        _hand = hand;
        _random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
    }

    // 손 패 전체 굴림
    public int RollHand()
    {
        if (_hand == null) return 0;

        IReadOnlyList<CardRuntime> hand = _hand.Cards;
        int rolled = 0;
        for(int i = 0; i < hand.Count; i++)
        {
            CardRuntime card = hand[i];
            if (card == null || card.data == null) continue;

            card.SetBasePower(RollFace(card.data));
            rolled++;
        }
        return rolled;
    }

    private int RollFace(CardData data)
    {
        int sides = data.DiceSides;
        ValidateDiceData(data, sides);
        if (sides <= 0) return 0;

        return _random.Next(1, sides + 1); 
    }

    // 주사위 스킨 스프라이트 검사
    private void ValidateDiceData(CardData data, int sides)
    {
        if (_warnedData.Contains(data)) return;

        int spriteCount = data.diceSkin != null && data.diceSkin.sprites != null ? data.diceSkin.sprites.Length : 0;
        if (sides > 0 && spriteCount == sides) return;
        Debug.LogWarning($"[DiceRollController] '{data.cardName}' 주사위 데이터 확인 필요 - 면 수 {sides}, 스킨 스프라이트 {spriteCount}장");
        _warnedData.Add(data);
    }
}
