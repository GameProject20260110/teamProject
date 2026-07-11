using VContainer;
using VContainer.Unity;

public class BossLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 기존 전투 씬과 공통되는 매니저들
        builder.RegisterComponentInHierarchy<GameManager>();
        builder.RegisterComponentInHierarchy<UiController>();
        builder.RegisterComponentInHierarchy<DiceManager>();
        builder.RegisterComponentInHierarchy<DicePanelManager>();
        builder.RegisterComponentInHierarchy<BattleManager>();
        builder.RegisterComponentInHierarchy<BattleInitalizer>();
        builder.RegisterComponentInHierarchy<EnemyAI>();
        builder.RegisterComponentInHierarchy<EnemyDeckHandler>();
        builder.RegisterComponentInHierarchy<DeckManager>();
        builder.RegisterComponentInHierarchy<BattleButton>();
        builder.RegisterComponentInHierarchy<DiceSpawnAnimation>();
        builder.RegisterComponentInHierarchy<EnemyDeathSequence>();
        builder.RegisterComponentInHierarchy<CardRevealAnimator>();
        builder.RegisterComponentInHierarchy<GameEndAnimation>();
        builder.RegisterComponentInHierarchy<DiceRoller>();
        builder.RegisterComponentInHierarchy<DefensePanelUI>();
        builder.RegisterComponentInHierarchy<AttackPanelUI>();
        builder.RegisterComponentInHierarchy<EffectManager>();
        builder.RegisterComponentInHierarchy<InventoryUI>();
        builder.RegisterComponentInHierarchy<ItemInventoryUI>();
        builder.RegisterComponentInHierarchy<ArtifactUIController>();
        builder.RegisterComponentInHierarchy<RoundIntro>();

        // 보스 씬 전용
        builder.RegisterComponentInHierarchy<BossStageController>();
        builder.RegisterComponentInHierarchy<BossStageIntro>();
        builder.RegisterComponentInHierarchy<RewardPanelUI>();
        builder.RegisterComponentInHierarchy<BossSceneDevBootstrapper>()
        .AsImplementedInterfaces()
        .AsSelf();
    }
}
