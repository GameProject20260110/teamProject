using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyAI/Balanced")]
public class BalancedAI : EnemyAIStrategy
{
    public override void PlaceDice(Dice[] hand)
    {
        PlaceByRole(hand); // 역할대로 배치
    }
}
