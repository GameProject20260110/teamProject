public class BattleSubscriptionManager
{
    public void Subscribe(BattleEventBus eventBus, BaseEnemyData enemyInfo)
    {
        foreach (var item in ItemManager.instance.items)
            item.OnEquip(eventBus);
        foreach (var item in ItemManager.instance.artifacts)
            item.OnEquip(eventBus);

        if (enemyInfo is BossDataSo bossData)
            bossData.RegisterAllGimmicks(eventBus);
    }

    public void Unsubscribe(BattleEventBus eventBus, BaseEnemyData enemyInfo)
    {
        foreach (var item in ItemManager.instance.items)
            item.OnUnequip(eventBus);
        foreach (var item in ItemManager.instance.artifacts)
            item.OnUnequip(eventBus);

        if (enemyInfo is BossDataSo bossData)
            bossData.UnregisterAllGimmicks(eventBus);
    }
}
