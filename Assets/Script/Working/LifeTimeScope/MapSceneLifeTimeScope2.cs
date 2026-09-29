using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MapSceneLifetimeScope : LifetimeScope
{
    [Header("SO ¾Ö¼Â")]
    [SerializeField] private EventObjectDatabase eventDatabase;

    protected override void Configure(IContainerBuilder builder)
    {
        
        builder.RegisterComponentInHierarchy<MapPathDrawer>();
        builder.RegisterComponentInHierarchy<MapCharacterMover>();
        builder.RegisterComponentInHierarchy<MapManager>();
        builder.RegisterComponentInHierarchy<MapCameraController>();

        builder.Register<MapPathfinder>(Lifetime.Singleton);
        builder.Register<MapIntroController>(Lifetime.Singleton);
        builder.Register<MapEventObjectSpawner>(Lifetime.Singleton);
        builder.Register<MapStageFlowController>(Lifetime.Singleton);

        builder.RegisterInstance(eventDatabase);
    }
}
