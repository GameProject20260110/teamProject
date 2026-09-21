using UnityEngine;

[CreateAssetMenu(menuName = "Event/HealEvent")]
public class HealEventSo : EventSo
{
    public int healAmount;

    public override void Execute()
    {
        ResourceManager.Instance.heart += healAmount;
    }
}
