using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using VContainer;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance { get; private set; }

    [Header("UI 연결")]
    public RectTransform rollArea;
    [Header("슬롯")]
    public Transform[] slots;
    public Transform[] enemySlots;
    public Transform[] enemyAttackSlots;
    public Transform[] enemyDefenseSlots;
    [Header("모듈")]
    public DiceRoller diceRoller;
    public bool isRolling => diceRoller != null && diceRoller.isRolling;
    public Dice[] panelDiceScript;
    public Dice[] enemyPanelDiceScript;


    [Inject]
    public void Construct()
    {
        Instance = this;
    }

    private void Awake()
    {
        panelDiceScript = new Dice[slots.Length];
        enemyPanelDiceScript = new Dice[enemySlots.Length];
    }

    public void PlaceDice(int slotIndex, DiceData data)
    {
        if (panelDiceScript[slotIndex] != null)
        {
            UIPoolManager.instance.Return(panelDiceScript[slotIndex].gameObject);
            panelDiceScript[slotIndex] = null;
        }
        GameObject obj = UIPoolManager.instance.Get(data.dicePrefab, slots[slotIndex], Vector2.zero);
        var draggable = obj.GetComponent<DraggableDice>();
        Dice dice = obj.GetComponent<Dice>();
        dice.Initialize(slotIndex, data);
        panelDiceScript[slotIndex] = dice;
    }

    #region 적 주사위 배치
    public void EnemyPlaceDice(int slotIndex, DiceData data)
    {
        if (enemyPanelDiceScript[slotIndex] != null)
        {
            UIPoolManager.instance.Return(enemyPanelDiceScript[slotIndex].gameObject);
            enemyPanelDiceScript[slotIndex] = null;
        }
        GameObject obj = UIPoolManager.instance.Get(data.dicePrefab, enemySlots[slotIndex], Vector2.zero);
        Dice dice = obj.GetComponent<Dice>();
        dice.Initialize(slotIndex, data);
        enemyPanelDiceScript[slotIndex] = dice;
    }

    public async UniTask EnemyPlaceAttackDice(int slotIndex, Dice dice)
    {
        await MoveDiceToSlot(dice, enemyAttackSlots[slotIndex]);
        BattleManager.Instance.SetEnemyAttackDice(dice);
    }

    public async UniTask EnemyPlaceDefenseDice(int slotIndex, Dice dice)
    {
        await MoveDiceToSlot(dice, enemyDefenseSlots[slotIndex]);
        BattleManager.Instance.SetEnemyDefenseDice(dice);
    }

    private async UniTask MoveDiceToSlot(Dice dice, Transform targetSlot, float duration = 0.4f)
    {
        await dice.transform
            .DOMove(targetSlot.position, duration)
            .SetEase(Ease.OutQuart)
            .AsyncWaitForCompletion();
        dice.transform.SetParent(targetSlot, true);
        dice.transform.localPosition = Vector3.zero;
    }
    #endregion

    public void ClearAllSlots()
    {
        for (int i = 0; i < panelDiceScript.Length; i++)
        {
            if (panelDiceScript[i] != null)
            {
                UIPoolManager.instance.Return(panelDiceScript[i].gameObject);
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
                UIPoolManager.instance.Return(enemyPanelDiceScript[i].gameObject);
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
        return enemyPanelDiceScript.Where(d => d != null).ToArray();
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
        for (int i = 0; i < panelDiceScript.Length; i++)
        {
            if (panelDiceScript[i] != null && panelDiceScript[i].gameObject.activeSelf) lastDiceSprite[i] = panelDiceScript[i].GetCurrentSprite();
        }
        return lastDiceSprite;
    }
}
