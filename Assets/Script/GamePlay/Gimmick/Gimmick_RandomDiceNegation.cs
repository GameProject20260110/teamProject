using UnityEngine;

[CreateAssetMenu(fileName = "Gimmick_RandomDiceNegation", menuName = "Gimmick/RandomDiceNegation")]
public class Gimmick_RandomDiceNegation : GimmickSo
{
    public override void ExecuteGimmick()
    {
        Debug.Log("주사위 하나의 효과를 무효화");
    }
}
