using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;

    [Header("아이템 종류")]
    [SerializeField] private BattleItemSo[] allItems;

    public List<BattleItemSo> items = new List<BattleItemSo>();
    public List<BattleItemSo> artifacts = new List<BattleItemSo>();

    private Dictionary<string, BattleItemSo> itemSearch;

    private const string SAVE_FILE = "Item.json";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        itemSearch = new Dictionary<string, BattleItemSo>();
        foreach (var item in allItems)
            itemSearch[item.itemName] = item;
    }

    public void Save()
    {
        ItemSaveData data = new ItemSaveData();
        foreach(var item in items)
            data.itemNames.Add(item.itemName);
        foreach (var artifact in artifacts)
            data.ArtifactNames.Add(artifact.itemName);

        SaveManager.instance.Save(data,SAVE_FILE);
    }

    public void Load()
    {
        if (SaveManager.instance == null) return;
        
        ItemSaveData data = SaveManager.instance.Load<ItemSaveData>(SAVE_FILE);

        items.Clear();
        artifacts.Clear();

        foreach(var itemName in data.itemNames)
        {
            BattleItemSo item = FindItem(itemName);
            if(item != null) items.Add(item);
        }

        foreach (var artifactName in data.ArtifactNames)
        {
            BattleItemSo artifact = FindItem(artifactName);
            if (artifact != null) artifacts.Add(artifact);
        }       
    }

    private BattleItemSo FindItem(string name)
    {
        itemSearch.TryGetValue(name,out var item);
        return item;
    }
}
