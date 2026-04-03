using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "itemSo",menuName = "Scriptable Object/ItemData")]
public class ItemSo : ScriptableObject
{
    public enum ItemTiming { Consumable, Reusable }
    
    public Sprite itemIcon;
    public int itemNum;
    public string itemName;
    public int gold;
    public int sell;

    [Range(1,5)]
    public int tier;
    public bool isConsumable;
    public ItemTiming timing;
    [TextArea]
    public string itemDesc;

    public virtual void Consumable() { } 

    public virtual void Reusable() { }

    //인게임 효과 아이템

    public virtual void RoundStart(List<DiceState> allDice, ref int totalSoce, List<ScoreEventData> events, int itemIndex = -1) { } // 지속 라운드 시작

    public virtual void RoundEnd() { } // 지속 라운드 끝


}
