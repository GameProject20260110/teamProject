using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    public static PlayerDeck instance;

    [Header("주사위 초기 덱")]
    [SerializeField] private PlayerDeckData defaultDeckData;

    [Header("주사위 종류")]
    [SerializeField] private DiceData[] allDiceData;

    [Header("가지고 있는 총 덱")]
    public List<DiceData> inventory = new List<DiceData>();

    private string SavePath => Application.persistentDataPath + "/deck.json";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        if (File.Exists(SavePath))
            Load();
        else
            inventory = new List<DiceData>(defaultDeckData.defultDeck);
    }
    
    public void AddDice(DiceData data)
    {
        inventory.Add(data);
        Save();
    }

    public void RemoveDice(DiceData data)
    {
        inventory.Remove(data);
        Save();
    }

    public void Save()
    {
        var saveData = new DeckSaveData();
        foreach (var dice in inventory)
            saveData.diceNums.Add(dice.diceNum);

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(SavePath, json);
    }

    public void Load()
    {
        string json = File.ReadAllText(SavePath);
        var saveData = JsonUtility.FromJson<DeckSaveData>(json);

        inventory.Clear();
        foreach (int num in saveData.diceNums)
        {
            DiceData data = FindDiceData(num);
            if (data != null) inventory.Add(data);
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        inventory = new List<DiceData>(defaultDeckData.defultDeck);
    }

    private DiceData FindDiceData(int diceNum)
    {
        foreach (var data in allDiceData)
            if (data.diceNum == diceNum) return data;
        return null;
    }

    [System.Serializable]
    private class DeckSaveData
    {
        public List<int> diceNums = new List<int>();
    }

    public bool ReplaceDefaultDice(DiceData newDice)
    {
        DiceData defaultDice = defaultDeckData.defultDeck[0];
        int index = inventory.FindIndex(d => d == defaultDice);

        if (index < 0) return false;

        inventory[index] = newDice;
        Save();
        return true;
    }
}
