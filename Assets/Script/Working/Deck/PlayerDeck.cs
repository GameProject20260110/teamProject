using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PlayerDeck : MonoBehaviour, IInitializable
{
    public static PlayerDeck Instance;

    [Header("덱 기본 데이터")]
    [SerializeField] private PlayerDeckData defaultDeckData;

    [Header("주사위 전체 데이터")]
    [SerializeField] private DiceData[] allDiceData;

    [Header("현재 가지고 있는 덱")]
    public List<DiceData> inventory = new List<DiceData>();

    private const string SAVE_FILE = "deck.json";
    private SaveManager _saveManager;

    [Inject]
    public void Construct(SaveManager saveManager)
    {
        _saveManager = saveManager;
        Instance = this;
    }

    public void Initialize()
    {
        if (_saveManager.HasSaveFile(SAVE_FILE))
            Load();
        else
            inventory = new List<DiceData>(defaultDeckData.defultDeck);
    }

    public void AddDice(DiceData data) { inventory.Add(data); Save(); }
    public void RemoveDice(DiceData data) { inventory.Remove(data); Save(); }

    public void Save()
    {
        var saveData = new DeckSaveData();
        foreach (var dice in inventory) saveData.diceNums.Add(dice.diceNum);
        _saveManager.Save(saveData, SAVE_FILE);
    }

    public void Load()
    {
        var saveData = _saveManager.Load<DeckSaveData>(SAVE_FILE);
        inventory.Clear();
        foreach (int num in saveData.diceNums)
        {
            DiceData data = FindDiceData(num);
            if (data != null) inventory.Add(data);
        }
    }

    public void DeleteSave()
    {
        _saveManager.Delete(SAVE_FILE);
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
