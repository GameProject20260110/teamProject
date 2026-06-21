using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyAI/Balanced")]
public class BalancedAI : EnemyAIStrategy
{
    public override async UniTask PlaceDice(Dice[] hand)
    {
        await PlaceByRole(hand); // 역할대로 배치
    }
}
