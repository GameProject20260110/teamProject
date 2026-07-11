using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ResourceManager : MonoBehaviour, IInitializable
{
    public static ResourceManager Instance;
    private const string SAVE_FILE = "Resource.json";
    public int gold;
    public int heart;

    private SaveManager _saveManager;

    [Inject]
    public void Construct(SaveManager saveManager)
    {
        _saveManager = saveManager;
        Instance = this;
    }

    public void Initialize()
    {
        Load();
    }

    private void InitDefault()
    {
        gold = 0;
        heart = 50;
    }

    public void AddGold(int gold)
    {
        this.gold += gold;
        Save();
    }

    public void Save()
    {
        ResourceSaveData resource = new ResourceSaveData { gold = gold, heart = heart };
        _saveManager.Save(resource, SAVE_FILE);
    }

    public void Load()
    {
        if (!_saveManager.HasSaveFile(SAVE_FILE))
        {
            InitDefault();
            return;
        }
        ResourceSaveData data = _saveManager.Load<ResourceSaveData>(SAVE_FILE);
        gold = data.gold;
        heart = data.heart;
    }

    public void ResetData()
    {
        InitDefault();
        Save();
    }
}
