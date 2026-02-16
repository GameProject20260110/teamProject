using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "itemSo",menuName = "Scriptable Object/ItemData")]
public class ItemSo : ScriptableObject
{
    
    public Sprite itemIcon;
    public int itemNum;
    public string itemName;
    public int gold;
    public int sell;
    public bool isConsumable;
    [TextArea]
    public string itemDesc;

    public virtual void Consumable() { } 

    public virtual void Reusable() { }

    //인게임 효과 아이템

    public virtual void RoundStart(List<DiceState> allDice, ref int totalSoce, List<ScoreEventData> events) { } // 지속 라운드 시작

    public virtual void RoundEnd() { } // 지속 라운드 끝


}
