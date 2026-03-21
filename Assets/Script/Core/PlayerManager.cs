using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public List<DiceData> dices = new List<DiceData>();
    public List<ItemSo> items = new List<ItemSo>();
    public DiceData extraDice;
    public bool[] SpecialSlots = new bool[6];
    public int gold;
    public int currentRound;
    public int heart;
    public bool isGameOver;

    private const int DiceSlotCount = 6;
    private const int ItemSlotCount = 7;

    public DiceData defaultDice;
    private DiceData[] allDices;
    private ItemSo[] allItems;
    

    private void Awake()
    {
        allDices = Resources.LoadAll<DiceData>("DiceDatas");
        allItems = Resources.LoadAll<ItemSo>("ItemDatas");

        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    private void InitDefault()
    {
        for (int i = 0; i < ItemSlotCount; i++)
        {
            items.Add(null);
        }
        for(int i = 0; i < DiceSlotCount; i++)
        {
            dices.Add(defaultDice);
            SpecialSlots[i] = (i == 0);
        }
        extraDice = defaultDice;
        gold = 10;
        currentRound = 1;
        heart = 3;
    }

    public void Save()
    {
        PlayerSaveData data = new PlayerSaveData();
        data.gold = gold;      
        data.currentRound = currentRound;
        data.heart = heart;

        data.extraDiceName = extraDice != null ? extraDice.name : "";
        data.specialSlots = this.SpecialSlots;

        foreach (var dice in dices)
            data.diceNames.Add(dice != null ? dice.name : "");
        foreach (var item in items)
            data.itemNames.Add(item != null ? item.name : "");
   
        string json = JsonUtility.ToJson(data, true);
        try
        {
            System.IO.File.WriteAllText(SavePath(), json);
        }
        catch(System.Exception ex)
        {
            Debug.LogError($" 저장 실패 : {ex.Message} (PlayerManager)");
        }

        
    }

    public void Load()
    {
        string path = SavePath();
        if (!System.IO.File.Exists(path))
        {
            InitDefault();
            return;
        }

        string json = System.IO.File.ReadAllText(path);
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

        gold = data.gold;
        heart = data.heart;
        currentRound = data.currentRound;
        extraDice = System.Array.Find(allDices, s => s.name == name);

        dices.Clear();
        items.Clear();

        this.SpecialSlots = data.specialSlots;
        if(data.specialSlots != null && data.specialSlots.Length == 6)
        {
            this.SpecialSlots = data.specialSlots;
        }
        else
        {
            this.SpecialSlots = new bool[6];
        }

         foreach (var name in data.diceNames)
         {
            var dice = System.Array.Find(allDices, s => s.name == name);
            if (dice != null) dices.Add(dice);
         }
        foreach (var name in data.itemNames)
        {
            if (string.IsNullOrEmpty(name))
            {
                items.Add(null);
                continue;
            }
            var item = System.Array.Find(allItems, s => s.name == name);
            if (item != null) items.Add(item);
        }
        if (dices.Count == 0) InitDefault();

        bool anyUnlocked = false;
        foreach (var slot in SpecialSlots)
        {
            if (slot)
            {
                anyUnlocked = true;
                break;
            }
        }
        if (!anyUnlocked) SpecialSlots[0] = true;
    }

    public void ResetData()
    {
        isGameOver = true;
        dices.Clear();
        items.Clear();    
        InitDefault();
        Save();
    }

    private string SavePath()
    {
        Debug.Log(Application.persistentDataPath);
        return Application.persistentDataPath + "/playerData.json";        
    }

    public void DeleteSave()
    {
        string path = SavePath();
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
    }
}
