using VContainer;
using VContainer.Unity;

public class TitleSceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<TitleView>();
        builder.Register<TitlePresenter>(Lifetime.Scoped).AsImplementedInterfaces();
    }
}
