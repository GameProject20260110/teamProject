using UnityEngine;
using VContainer;
using VContainer.Unity;

public class DeckSceneLifetimeScope : LifetimeScope
{
    [Header("씬 로컬 에셋 참조")]
    [SerializeField] private CardDatabase cardDatabase;
    [SerializeField] private DeckManagementConfig config;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(cardDatabase);
        builder.RegisterInstance(config);

        builder.Register<DeckEditSession>(Lifetime.Scoped);

        builder.RegisterComponentInHierarchy<DeckListPanelController>();
        builder.RegisterComponentInHierarchy<DeckPopupService>();
        builder.RegisterComponentInHierarchy<DeckManagementSceneController>();

        RegisterIfPresent<DeckEditScreenController>(builder);
        RegisterIfPresent<DeckGridController>(builder);
        RegisterIfPresent<OwnedCardListController>(builder);
        RegisterIfPresent<CardDetailPanelController>(builder);
    }

    private void RegisterIfPresent<T>(IContainerBuilder builder) where T : Component
    {
        foreach(GameObject root in gameObject.scene.GetRootGameObjects())
        {
            T found = root.GetComponentInChildren<T>(true);
            if (found == null) continue;
            builder.RegisterComponent(found);
            return;
        }
    }
}
