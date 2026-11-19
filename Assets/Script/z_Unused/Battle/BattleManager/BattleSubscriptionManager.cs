public class BattleSubscriptionManager
{
    private readonly ItemManager _itemManager;

    public BattleSubscriptionManager(ItemManager itemManager)
    {
        _itemManager = itemManager;
    }

    public void Subscribe(BattleEventBus eventBus, BaseEnemyData enemyInfo)
    {
        foreach (var item in _itemManager.items)
            item.OnEquip(eventBus);
        foreach (var item in _itemManager.artifacts)
            item.OnEquip(eventBus);

        if (enemyInfo is BossDataSo bossData)
            bossData.RegisterAllGimmicks(eventBus);
    }

    public void Unsubscribe(BattleEventBus eventBus, BaseEnemyData enemyInfo)
    {
        foreach (var item in _itemManager.items)
            item.OnUnequip(eventBus);
        foreach (var item in _itemManager.artifacts)
            item.OnUnequip(eventBus);

        if (enemyInfo is BossDataSo bossData)
            bossData.UnregisterAllGimmicks(eventBus);
    }
}
