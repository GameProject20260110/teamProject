using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BattleItemList", menuName = "Scriptable Object/BattleItemList")]
public class BattleItemList : ScriptableObject
{
    public List<BattleItemSo> ArtifactDatas;
    public List<BattleItemSo> ItemDatas;
}
