using VContainer;
using VContainer.Unity;

public class MapSceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<MapPathDrawer>();
        builder.RegisterComponentInHierarchy<MapCharacterMover>();
        builder.RegisterComponentInHierarchy<MapManager>();

        builder.Register<MapPathfinder>(Lifetime.Singleton);
        builder.Register<MapIntroController>(Lifetime.Singleton);
        builder.Register<MapStageFlowController>(Lifetime.Singleton);
    }
}
