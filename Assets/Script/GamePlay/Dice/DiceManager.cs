using Cysharp.Threading.Tasks;
using UnityEngine;

public class DiceManager : MonoBehaviour
{
    public static DiceManager instance;
  
    [Header("UI 연결")]
    public RectTransform rollArea;
    public PanelEffect panelEffect;

    [Header("슬롯")]
    public Transform[] slots;
    public Transform[] enemySlots;
    public Transform[] enemyAttackSlots;
    public Transform[] enemyDefenseSlots;

    [Header("모듈")]
    public DiceRoller diceRoller;

    public bool isRolling => diceRoller != null && diceRoller.isRolling;

    public Dice[] panelDiceScript; //{ get; private set; }
    public Dice[] enemyPanelDiceScript;

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
        panelDiceScript = new Dice[slots.Length];
        enemyPanelDiceScript = new Dice[enemySlots.Length];
    }

    void Start()
    {
        if(GameManager.instance != null) GameManager.instance.diceManager = this;
    }

    public void PlaceDice(int slotIndex, DiceData data)
    {
        if (panelDiceScript[slotIndex] != null)
        {
            ObjectPool.instance.Return(panelDiceScript[slotIndex].gameObject);
            panelDiceScript[slotIndex] = null;
        }

        GameObject obj = ObjectPool.instance.Get(data.dicePrefab);
        obj.transform.SetParent(slots[slotIndex], false);
        obj.transform.localPosition = Vector3.zero;

        var draggable = obj.GetComponent<DraggableDice>();
        if (draggable != null)
            draggable.panelEffect = panelEffect;

        Dice dice = obj.GetComponent<Dice>();
        dice.Initialize(slotIndex, data);
        panelDiceScript[slotIndex] = dice;
    }


    #region 적 주사위 배치

    public void EnemyPlaceDice(int slotIndex, DiceData data)
    {
        if (enemyPanelDiceScript[slotIndex] != null)
        {
            ObjectPool.instance.Return(enemyPanelDiceScript[slotIndex].gameObject);
            enemyPanelDiceScript[slotIndex] = null;
        }

        GameObject obj = ObjectPool.instance.Get(data.dicePrefab);
        obj.transform.SetParent(enemySlots[slotIndex], false);
        obj.transform.localPosition = Vector3.zero;

        Dice dice = obj.GetComponent<Dice>();
        dice.Initialize(slotIndex, data);
        enemyPanelDiceScript[slotIndex] = dice;
    }

    public void EnemyPlaceAttackDice(int slotIndex, Dice dice)
    {
        dice.transform.SetParent(enemyAttackSlots[slotIndex], false);
        dice.transform.localPosition = Vector3.zero;
        BattleManager.instance.SetEnemyAttackDice(dice);
    }

    public void EnemyPlaceDefenseDice(int slotIndex, Dice dice)
    {
        dice.transform.SetParent(enemyDefenseSlots[slotIndex], false);
        dice.transform.localPosition = Vector3.zero;
        BattleManager.instance.SetEnemyDefenseDice(dice);
    }

    #endregion



    public void ClearAllSlots()
    {
        for (int i = 0; i < panelDiceScript.Length; i++)
        {
            if (panelDiceScript[i] != null)
            {
                ObjectPool.instance.Return(panelDiceScript[i].gameObject);
                panelDiceScript[i] = null;
            }
        }
    }

    public void ClearEnemyAllSlots()
    {
        for (int i = 0; i < enemyPanelDiceScript.Length; i++)
        {
            if (enemyPanelDiceScript[i] != null)
            {
                ObjectPool.instance.Return(enemyPanelDiceScript[i].gameObject);
                enemyPanelDiceScript[i] = null;
            }
        }
    }

    public async UniTask<Dice[]> StartEnemyRolling()
    {
        Dice[] allDice = GetEnemyAllDice();
        await diceRoller.StartRoll(allDice, rollArea);
        return allDice;
    }

    public Dice[] GetEnemyAllDice()
    {
        Dice[] allDice = new Dice[enemyPanelDiceScript.Length];
        enemyPanelDiceScript.CopyTo(allDice, 0);
        return allDice;
    }

    public async UniTask<Dice[]> StartRolling()
    {
        Dice[] allDice = GetAllDice();
        await diceRoller.StartRoll(allDice, rollArea);
        return allDice;
    }

    public Dice[] GetAllDice()
    {
        Dice[] allDice = new Dice[panelDiceScript.Length];
        panelDiceScript.CopyTo(allDice, 0);
        return allDice;
    }
    
    public Sprite[] GetLastDiceSprites()
    {
        Sprite[] lastDiceSprite = new Sprite[panelDiceScript.Length];
        for(int i = 0; i < panelDiceScript.Length; i++)
        {
            if (panelDiceScript[i] != null && panelDiceScript[i].gameObject.activeSelf) lastDiceSprite[i] = panelDiceScript[i].GetCurrentSprite();
        }
        return lastDiceSprite;
    }
}
