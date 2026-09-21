using VContainer;
using VContainer.Unity;

public class EventMapLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<EventMapManager>();
    }
}
