using VContainer;
using VContainer.Unity;

public class MapSceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<MapManager>();
        builder.RegisterComponentInHierarchy<MapPathDrawer>();
        builder.RegisterComponentInHierarchy<MapCameraController>();
        builder.RegisterComponentInHierarchy<MapIntroController>();
    }
}
