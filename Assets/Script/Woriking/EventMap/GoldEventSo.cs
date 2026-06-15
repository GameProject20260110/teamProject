using UnityEngine;

[CreateAssetMenu(menuName = "Event/GoldEvent")]
public class GoldEventSo : EventSo
{
    public int goldAmount;

    public override void Execute()
    {
        ResourceManager.instance.gold += goldAmount;
    }
}
