using VContainer;
using VContainer.Unity;

public class RoundLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<RoundStageManager>();
    }
}
