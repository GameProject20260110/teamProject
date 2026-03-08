using UnityEngine;

[CreateAssetMenu(fileName = "Gimmick_NoScoreDice", menuName = "Gimmick/NoScoreDice")]
public class Gimmick_NoScoreDice : GimmickSo
{
    public override void ExecuteGimmick()
    {
        Debug.Log("효과 없는 주사위는 점수 획득 불가");
    }
}
