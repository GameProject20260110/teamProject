using UnityEngine;

[CreateAssetMenu(fileName = "Gimmick_NoReroll", menuName = "Gimmick/NoReroll")]
public class Gimmick_NoReroll : GimmickSo
{
    public override void ExecuteGimmick()
    {
        if(GameManager.instance != null)
        {
            PlayerManager.instance.gameRerollCount = 0;
            GameManager.instance.InitializeRoundData();
        }
        Debug.Log("리롤 불가");
    }
}
