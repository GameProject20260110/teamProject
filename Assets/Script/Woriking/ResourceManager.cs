using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;
    private const string SAVE_FILE = "Resource.json";

    public Sprite PlayerImage;

    public int gold;
    public int heart;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitDefault()
    {
        gold = 0;
        heart = 50;
    }

    public void AddGold(int gold)
    {
        this.gold += gold;
        Debug.Log($"현재 gold : {this.gold}");
        Save();
    }

    public void Save()
    {
        ResourceSaveData resource = new ResourceSaveData
        {
            gold = gold,
            heart = heart,
        };

        SaveManager.instance.Save(resource, SAVE_FILE);
    }

    public void Load()
    {
        if (SaveManager.instance == null || !SaveManager.instance.HasSaveFile(SAVE_FILE))
        {
            Debug.LogWarning("기본값 사용");
            InitDefault();
            return;
        }

        ResourceSaveData data = SaveManager.instance.Load<ResourceSaveData>(SAVE_FILE);

        gold = data.gold;
        heart = data.heart;

    }

    public void ResetData()
    {
        InitDefault();
        Save();
    }
}
