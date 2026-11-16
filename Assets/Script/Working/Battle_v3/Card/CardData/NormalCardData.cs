using UnityEngine;

[CreateAssetMenu(fileName = "NormalCardData", menuName = "CardData/NormalCardData")]
public class NormalCardData : CardData
{
    public override void ApplyEffect(CardRuntime runtime)
    {
        // 효과 없음 기본 카드
    }
}
