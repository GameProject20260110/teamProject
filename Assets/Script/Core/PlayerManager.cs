using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public List<DiceData> dices = new List<DiceData>();
    public List<ItemSo> items = new List<ItemSo>();
    public DiceData extraDice;
    public bool[] SpecialSlots = new bool[6];
    public int ShopLevel;
    public int ShopCount;
    public int tempExtraSlotsCount = 0;
    public int gold;
    public int currentRound;
    public int heart;
    public int gameRerollCount;
    public bool isFirstRoll;
    public bool isGameOver;
    public Sprite playerImage;

    private const int DiceSlotCount = 6;
    private const int ItemSlotCount = 7;
    private const string SAVE_FILE = "playerData.json";

    public DiceData defaultDice;
    private DiceData[] allDices;
    private ItemSo[] allItems;
    private GimmickSo[] allGimmicks;
    public List<GimmickSo> pendingGimmicks = new List<GimmickSo>();

    private void Awake()
    {
        
        if(instance == null)
        {
            instance = this;
            allDices = Resources.LoadAll<DiceData>("DiceDatas");
            allItems = Resources.LoadAll<ItemSo>("ItemDatas");
            allGimmicks = Resources.LoadAll<GimmickSo>("GimmickData");
            gameRerollCount = 1;
            isFirstRoll = true;
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    private void InitDefault()
    {
        dices.Clear();
        items.Clear();
        pendingGimmicks.Clear();

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
        gold = 100;
        gameRerollCount = 1;
        isFirstRoll = true;
        currentRound = 1;
        heart = 50;
        ShopLevel = 1;
        ShopCount = 3;
    }

    public void Save()
    {
        PlayerSaveData data = new PlayerSaveData
        {
            gold = gold,
            currentRound = currentRound,
            heart = heart,
            ShopLevel = ShopLevel,
            extraDiceName = extraDice != null ? extraDice.name : "",
            specialSlots = this.SpecialSlots,
            isFirstRoll = isFirstRoll,
            ShopCount = ShopCount
        };

        foreach (var dice in dices)
            data.diceNames.Add(dice != null ? dice.name : "");
        foreach (var item in items)
            data.itemNames.Add(item != null ? item.name : "");
        foreach (var gimmick in pendingGimmicks)
            data.pendingGimmickNames.Add(gimmick != null ? gimmick.name : "");

        SaveManager.instance.Save(data, SAVE_FILE);
    }

    public void Load()
    {
        if (SaveManager.instance == null)
        {
            Debug.LogWarning("SaveManager가 없습니다. 기본값 사용");
            InitDefault();
            return;
        }

        if (!SaveManager.instance.HasSaveFile(SAVE_FILE))
        {
            InitDefault();
            return;
        }

        PlayerSaveData data = SaveManager.instance.Load<PlayerSaveData>(SAVE_FILE);

        gold = data.gold;
        heart = data.heart;
        currentRound = data.currentRound;
        ShopLevel = data.ShopLevel;
        ShopCount = data.ShopCount;
        extraDice = System.Array.Find(allDices, s => s.name == name);
        isFirstRoll = data.isFirstRoll;

        

        this.SpecialSlots = data.specialSlots;

        if(data.specialSlots != null && data.specialSlots.Length == 6)
        {
            this.SpecialSlots = data.specialSlots;
        }
        else
        {
            this.SpecialSlots = new bool[6];
        }

        dices.Clear();
        foreach (var name in data.diceNames)
        {
           var dice = System.Array.Find(allDices, s => s.name == name);
           if (dice != null) dices.Add(dice);
        }

        items.Clear();
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

        if(data.pendingGimmickNames != null)
        {
            foreach(var name in data.pendingGimmickNames)
            {
                if (string.IsNullOrEmpty(name)) continue;
                var gimmick = System.Array.Find(allGimmicks, g => g.name == name);
                if (gimmick != null) pendingGimmicks.Add(gimmick);
            }
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
        InitDefault();
        Save();
    }
}
