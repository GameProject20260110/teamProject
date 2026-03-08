using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Gimmick_DoubleTargetScore", menuName = "Gimmick/DoubleTargetScore")]
public class Gimmick_DoubleTargetScore : GimmickSo
{
    public override void ExecuteGimmick()
    {
        if(RoundManager.instance != null)
        {
            RoundManager.instance.targetScore *= 2;
            GameManager.instance?.NotifyAllUI();
            Debug.Log($"목표 점수 x2 -> 현재 목표 점수 : {RoundManager.instance.targetScore}");
        }
    }
}
