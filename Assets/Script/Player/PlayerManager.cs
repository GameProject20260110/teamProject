using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public List<DiceData> dices = new List<DiceData>();
    public List<ItemSo> items = new List<ItemSo>();
    public int gold;
    public int level;
    public int currentRound;
    public int maxLives;
    public int currentLives;

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

    public void PushPlayerDices(DiceData Dice)
    {
        for (int i = 0; i < dices.Count; i++)
        {
            if (dices[i] == null)
            {
                dices[i] = Dice;
                return;
            }

        }

    }

    public void PullPlayerDices(DiceData Dice)
    {
        for (int i = 0; i < dices.Count; i++)
        {
            if (dices[i] == Dice)
            {
                dices[i] = null;
                return;
            }

        }
    }

    public void PushPlayerDices(DiceData Dice, int index)
    {     
        dices[index] = Dice;
    }

    public void PullPlayerDices(DiceData Dice, int index)
    {
        if (dices[index] == Dice)
        {
            dices[index] = null;
        }
    }

    public void PushPlayerItems(ItemSo item)
    {
        if (PlayerManager.instance.items.Count > 7) return;
        items.Add(item);

    }

    public void PullPlayerItems(ItemSo item)
    {
        items.Remove(item);

    }

    public void Save()
    {
        PlayerSaveData data = new PlayerSaveData();
        data.gold = gold;
        data.level = level;
        data.currentRound = currentRound;
        data.maxLives = maxLives;
        data.currentLives = currentLives;

        foreach (var dice in dices)
            data.diceNames.Add(dice.name);
        foreach (var item in items)
            data.itemNames.Add(item.name);
        
        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(SavePath(), json);
    }

    public void Load()
    {
        string path = SavePath();
        if (!System.IO.File.Exists(path)) return;

        string json = System.IO.File.ReadAllText(path);
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

        gold = data.gold;
        level = data.level;
        currentRound = data.currentRound;
        maxLives = data.maxLives;
        currentLives = data.currentLives;


        dices.Clear();
        items.Clear();

        foreach(var name in data.diceNames)
        {
            var dice = System.Array.Find(allDices, s=>s.name == name);
            if (dice != null) dices.Add(dice);
        }
        foreach (var name in data.itemNames)
        {
            var item = System.Array.Find(allItems, s => s.name == name);
            if (item != null) items.Add(item);
        }
    }

    private string SavePath()
    {
        Debug.Log(Application.persistentDataPath);
        return Application.persistentDataPath + "/playerData.json";
        
    }
}
