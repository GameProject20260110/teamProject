using VContainer;
using VContainer.Unity;

public class BossLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 핵심 매니저
        builder.RegisterComponentInHierarchy<GameManager>();
        builder.RegisterComponentInHierarchy<BattleManager>();
        builder.RegisterComponentInHierarchy<BattleInitalizer>();
        builder.RegisterComponentInHierarchy<UiController>();

        // 주사위 / 덱
        builder.RegisterComponentInHierarchy<DiceManager>();
        builder.RegisterComponentInHierarchy<DicePanelManager>();
        builder.RegisterComponentInHierarchy<DeckManager>();
        builder.RegisterComponentInHierarchy<DiceRoller>();
        builder.RegisterComponentInHierarchy<DiceSpawnAnimation>();
        builder.RegisterComponentInHierarchy<BattleButton>();

        // 적 AI
        builder.RegisterComponentInHierarchy<EnemyAI>();
        builder.RegisterComponentInHierarchy<EnemyDeckHandler>();
        builder.RegisterComponentInHierarchy<EnemyDeathSequence>();

        // 연출/이펙트
        builder.RegisterComponentInHierarchy<CharacterAppearEffect>();
        builder.RegisterComponentInHierarchy<CardAppearEffect>();
        builder.RegisterComponentInHierarchy<CardRevealAnimator>();
        builder.RegisterComponentInHierarchy<GameEndAnimation>();
        builder.RegisterComponentInHierarchy<EffectManager>();
        builder.RegisterComponentInHierarchy<RoundIntro>();

        // UI 패널
        builder.RegisterComponentInHierarchy<DefensePanelUI>();
        builder.RegisterComponentInHierarchy<AttackPanelUI>();
        builder.RegisterComponentInHierarchy<InventoryUI>();
        builder.RegisterComponentInHierarchy<ItemInventoryUI>();
        builder.RegisterComponentInHierarchy<ArtifactUIController>();
        builder.RegisterComponentInHierarchy<RewardPanelUI>();

        // 보스 씬 전용
        builder.RegisterComponentInHierarchy<BossStageController>();
        builder.RegisterComponentInHierarchy<BossStageIntro>();

        // 개발용
        builder.RegisterComponentInHierarchy<BossSceneDevBootstrapper>()
            .AsImplementedInterfaces()
            .AsSelf();
    }
}