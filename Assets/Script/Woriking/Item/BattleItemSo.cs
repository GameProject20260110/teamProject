using UnityEngine;

[CreateAssetMenu(fileName = "BattleItemSo", menuName = "Scriptable Object/BattleItemData")]
public abstract class BattleItemSo : ScriptableObject
{
    public Sprite itemIcon;
    public string itemName;
    public bool isConsumable;
    [TextArea]
    public string itemDesc;

    public abstract void OnUse(BattleContext ctx);
}