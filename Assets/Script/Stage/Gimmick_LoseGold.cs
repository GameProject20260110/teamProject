using UnityEngine;

[CreateAssetMenu(fileName = "Gimmick_LoseGold", menuName = "Gimmick/LoseGold")]
public class Gimmick_LoseGold : GimmickSo
{
    public override void ExecuteGimmick()
    {
        if (PlayerManager.instance == null) return;

        if(Random.value > 0.50f)
        {
            int lost = PlayerManager.instance.gold;
            PlayerManager.instance.gold = 0;
            Debug.Log("골드 전부 삭제");
        }
        else
        {
            int currentGold = PlayerManager.instance.gold;
            int lostAmount = (currentGold % 2 != 0) ? (currentGold + 1) / 2 : currentGold / 2;
            PlayerManager.instance.gold -= lostAmount;
            if (PlayerManager.instance.gold < 0) PlayerManager.instance.gold = 0;
            Debug.Log("골드 절반 삭제");
        }

        GameManager.instance?.NotifyAllUI();
    }
}
