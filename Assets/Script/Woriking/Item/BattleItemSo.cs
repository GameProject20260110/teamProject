using UnityEngine;

[CreateAssetMenu(fileName = "BattleItemSo", menuName = "Scriptable Object/BattleItemData")]
public abstract class BattleItemSo : ScriptableObject
{
    public Sprite itemIcon;
    public string itemName;
    public bool isConsumable;
    public bool isArtifact; // 아티팩트 아이템 구분
    public int gold;
    [TextArea]
    public string itemDesc;

    public abstract void OnUse(DiceContext ctx);
    public virtual void OnEquip(BattleEventBus bus) { }
    public virtual void OnUnequip(BattleEventBus bus) { }
}