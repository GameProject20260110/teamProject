using UnityEngine;

[CreateAssetMenu(menuName = "Event/ArtifactEvent")]
public class ArtifactEventSo : EventSo
{
    public BattleItemList itemList;

    public override void Execute()
    {
        int randNum = Random.Range(0,itemList.ArtifactDatas.Count);
        ItemManager.instance.artifacts.Add(itemList.ArtifactDatas[randNum]);
    }
}
