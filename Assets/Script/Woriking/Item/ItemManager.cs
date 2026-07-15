using UnityEngine;
using System.Collections.Generic;
using VContainer;

public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;

    [Header("아이템 종류")]
    [SerializeField] private BattleItemSo[] allItems;
    public List<BattleItemSo> items = new List<BattleItemSo>();
    public List<BattleItemSo> artifacts = new List<BattleItemSo>();
    private Dictionary<string, BattleItemSo> itemSearch;
    private const string SAVE_FILE = "Item.json";

    private SaveManager _saveManager;

    [Inject]
    public void Construct(SaveManager saveManager)
    {
        _saveManager = saveManager;

        itemSearch = new Dictionary<string, BattleItemSo>();
        foreach (var item in allItems)
            itemSearch[item.itemName] = item;
    }

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void Save()
    {
        ItemSaveData data = new ItemSaveData();
        foreach(var item in items)
            data.itemNames.Add(item.itemName);
        foreach (var artifact in artifacts)
            data.ArtifactNames.Add(artifact.itemName);

        _saveManager.Save(data,SAVE_FILE);
    }

    public void Load()
    {        
        ItemSaveData data = _saveManager.Load<ItemSaveData>(SAVE_FILE);

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

    public void AddItem(BattleItemSo item)
    {
        if (item.isArtifact)
        {
            artifacts.Add(item);
            ArtifactUIController.instance?.AddArtifactIcon(item);
        }
        else
            items.Add(item);
        Save();
    }

    private BattleItemSo FindItem(string name)
    {
        itemSearch.TryGetValue(name,out var item);
        return item;
    }
}
