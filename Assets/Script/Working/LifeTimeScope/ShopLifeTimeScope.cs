using VContainer;
using VContainer.Unity;

public class ShopSceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<PlayerShopManager>();
        builder.RegisterComponentInHierarchy<ShopUIManager>();
        builder.RegisterComponentInHierarchy<ShopPanelAnimator>();
        builder.RegisterComponentInHierarchy<PopupManager>();
    }
}
