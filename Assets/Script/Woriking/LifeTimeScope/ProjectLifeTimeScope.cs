using VContainer;
using VContainer.Unity;
using UnityEngine;

public class ProjectLifetimeScope : LifetimeScope
{
    [SerializeField] private SettingsManager settingsManagerPrefab;
    [SerializeField] private AudioManager audioManagerPrefab;
    [SerializeField] private SaveManager saveManagerPrefab;
    [SerializeField] private SceneController sceneControllerPrefab;
    [SerializeField] private ItemManager itemManagerPrefab;
    [SerializeField] private PlayerDeck playerDeckPrefab;
    [SerializeField] private ResourceManager resourceManagerPrefab;
    [SerializeField] private BattleDataManager battleDataManagerPrefab;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInNewPrefab(settingsManagerPrefab, Lifetime.Singleton).DontDestroyOnLoad();
        builder.RegisterComponentInNewPrefab(audioManagerPrefab, Lifetime.Singleton).DontDestroyOnLoad();
        builder.RegisterComponentInNewPrefab(sceneControllerPrefab, Lifetime.Singleton).DontDestroyOnLoad();
        builder.RegisterComponentInNewPrefab(itemManagerPrefab, Lifetime.Singleton).DontDestroyOnLoad();
        builder.RegisterComponentInNewPrefab(battleDataManagerPrefab, Lifetime.Singleton).DontDestroyOnLoad();

        builder.Register<SaveManager>(Lifetime.Singleton);
        builder.Register<MapSaveLoad>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        builder.RegisterComponentInNewPrefab(playerDeckPrefab, Lifetime.Singleton)
            .DontDestroyOnLoad()
            .AsImplementedInterfaces()
            .AsSelf();

        builder.RegisterComponentInNewPrefab(resourceManagerPrefab, Lifetime.Singleton)
            .DontDestroyOnLoad()
            .AsImplementedInterfaces()
            .AsSelf();
    }
}