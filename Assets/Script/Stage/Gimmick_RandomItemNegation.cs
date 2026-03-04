using UnityEngine;

[CreateAssetMenu(fileName = "Gimmick_RandomItemNegation", menuName = "Gimmick/RandomItemNegation")]
public class Gimmick_RandomItemNegation : GimmickSo
{
    public override void ExecuteGimmick()
    {
        Debug.Log("아이템의 효과를 25%의 확률로 무시한다.");
    }
}
