using VContainer;
using VContainer.Unity;
using UnityEngine;

public class ProjectLifetimeScope : LifetimeScope
{
    [SerializeField] private SettingsManager settingsManagerPrefab;
    [SerializeField] private AudioManager audioManagerPrefab;
    [SerializeField] private SaveManager saveManagerPrefab;
    [SerializeField] private SceneController sceneControllerPrefab;    
    [SerializeField] private PlayerDeck playerDeckPrefab;
    [SerializeField] private ResourceManager resourceManagerPrefab;
    [SerializeField] private CardDatabase cardDatabase;
    
    // ��� ���ɼ� ����
    [SerializeField] private BattleDataManager battleDataManagerPrefab;
    [SerializeField] private PlayerHudManager playerHudManagerPrefab;
    [SerializeField] private ItemManager itemManagerPrefab;


    [Header("�� ���� �ý��� (�ű� ī��)")]
    [SerializeField] private PlayerCardCollection playerCardCollectionPrefab;
    [SerializeField] private DeckRepository DeckRepositoryPrefab;
    [SerializeField] private CardDatabase cardDatabase;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInNewPrefab(settingsManagerPrefab, Lifetime.Singleton).DontDestroyOnLoad();
        builder.RegisterComponentInNewPrefab(audioManagerPrefab, Lifetime.Singleton).DontDestroyOnLoad();
        builder.RegisterComponentInNewPrefab(sceneControllerPrefab, Lifetime.Singleton).DontDestroyOnLoad();
        builder.RegisterComponentInNewPrefab(itemManagerPrefab, Lifetime.Singleton).DontDestroyOnLoad();
        builder.RegisterComponentInNewPrefab(battleDataManagerPrefab, Lifetime.Singleton).DontDestroyOnLoad();
        builder.RegisterComponentInNewPrefab(playerHudManagerPrefab, Lifetime.Singleton)
            .DontDestroyOnLoad()
            .AsImplementedInterfaces()
            .AsSelf();

        builder.Register<SaveManager>(Lifetime.Singleton);
        builder.Register<MapSaveLoad>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        builder.Register<MapProgressStore>(Lifetime.Singleton);
        if (cardDatabase != null) builder.RegisterInstance(cardDatabase);
        else Debug.LogWarning("[ProjectLifetimeScope] cardDatabase�� ��� �ֽ��ϴ�.");

            //builder.RegisterComponentInNewPrefab(playerDeckPrefab, Lifetime.Singleton)
            //    .DontDestroyOnLoad()
            //    .AsImplementedInterfaces()
            //    .AsSelf();

            builder.RegisterComponentInNewPrefab(resourceManagerPrefab, Lifetime.Singleton)
                .DontDestroyOnLoad()
                .AsImplementedInterfaces()
                .AsSelf();

        
        builder.RegisterComponentInNewPrefab(DeckRepositoryPrefab, Lifetime.Singleton)
            .DontDestroyOnLoad()
            .AsImplementedInterfaces()
            .AsSelf();

        builder.RegisterComponentInNewPrefab(playerCardCollectionPrefab, Lifetime.Singleton)
            .DontDestroyOnLoad()
            .AsImplementedInterfaces()
            .AsSelf();
        builder.RegisterInstance(cardDatabase);
    }
}