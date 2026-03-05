using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Gimmick_LoseRandomItem", menuName = "Gimmick/NegateNormalDice")]
public class Gimmick_LoseRandomItem : GimmickSo
{
    public override void ExecuteGimmick()
    {
        List<ItemSo> items = GetPlayerItems();
        if(items == null || items.Count == 0)
        {
            Debug.Log("소모 할 아이템이 없음");
        }
        int rand = Random.Range(0, items.Count);
        Debug.Log($"{items[rand].itemName}이 사라졌습니다.");
        items.RemoveAt(rand);
    }

    private List<ItemSo> GetPlayerItems()
    {
        if(PlayerManager.instance != null)
        {
            return PlayerManager.instance.items;
        }
        return null;
    }
}
