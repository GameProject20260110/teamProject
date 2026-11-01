using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ShopSceneLifetimeScope : LifetimeScope
{
    [SerializeField] private ShopConfig shopConfig;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<PlayerShopManager>();
        builder.RegisterComponentInHierarchy<ShopUIManager>();
        builder.RegisterComponentInHierarchy<PopupManager>();

        builder.RegisterInstance(shopConfig);
        builder.Register<ShopSession>(Lifetime.Scoped);
    }
}
