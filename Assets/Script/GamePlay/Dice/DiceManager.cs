using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using VContainer;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance { get; private set; }

    [Header("슬롯 컨테이너")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private Transform enemySlotContainer;
    [SerializeField] private Transform enemyAttackSlotContainer;
    [SerializeField] private Transform enemyDefenseSlotContainer;

    [Header("슬롯 (자동 채움, 인스펙터에서 손대지 마세요)")]
    private Transform[] slots;
    private Transform[] enemySlots;
    private Transform[] enemyAttackSlots;
    private Transform[] enemyDefenseSlots;

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
        slots = GetSlotsFromContainer(slotContainer);
        enemySlots = GetSlotsFromContainer(enemySlotContainer);
        enemyAttackSlots = GetSlotsFromContainer(enemyAttackSlotContainer);
        enemyDefenseSlots = GetSlotsFromContainer(enemyDefenseSlotContainer);

        panelDiceScript = new Dice[slots.Length];
        enemyPanelDiceScript = new Dice[enemySlots.Length];
    }

    private Transform[] GetSlotsFromContainer(Transform container)
    {
        if (container == null)
        {
            Debug.LogWarning("[DiceManager] 슬롯 컨테이너가 비어있습니다.");
            return new Transform[0];
        }

        Transform[] result = new Transform[container.childCount];
        for (int i = 0; i < container.childCount; i++)
            result[i] = container.GetChild(i);
        return result;
    }

    public void PlaceDice(int slotIndex, DiceData data)
    {
        if (panelDiceScript[slotIndex] != null)
        {
            WorldPoolManager.instance.Return(panelDiceScript[slotIndex].gameObject);
            panelDiceScript[slotIndex] = null;
        }

        GameObject obj = WorldPoolManager.instance.Get(
            data.dicePrefab,
            slots[slotIndex].position,
            Quaternion.identity,
            slots[slotIndex]);

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
            WorldPoolManager.instance.Return(enemyPanelDiceScript[slotIndex].gameObject);
            enemyPanelDiceScript[slotIndex] = null;
        }

        GameObject obj = WorldPoolManager.instance.Get(
            data.dicePrefab,
            enemySlots[slotIndex].position,
            Quaternion.identity,
            enemySlots[slotIndex]);

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
        dice.transform.SetParent(targetSlot, false);
        dice.transform.localPosition = Vector3.zero;
    }
    #endregion

    public void ClearAllSlots()
    {
        for (int i = 0; i < panelDiceScript.Length; i++)
        {
            if (panelDiceScript[i] != null)
            {
                WorldPoolManager.instance.Return(panelDiceScript[i].gameObject);
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
                WorldPoolManager.instance.Return(enemyPanelDiceScript[i].gameObject);
                enemyPanelDiceScript[i] = null;
            }
        }
    }

    public async UniTask<Dice[]> StartEnemyRolling()
    {
        Dice[] allDice = GetEnemyAllDice();
        await diceRoller.StartRoll(allDice);
        return allDice;
    }

    public Dice[] GetEnemyAllDice() => enemyPanelDiceScript.Where(d => d != null).ToArray();

    public async UniTask<Dice[]> StartRolling()
    {
        Dice[] allDice = GetAllDice();
        await diceRoller.StartRoll(allDice);
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
            if (panelDiceScript[i] != null && panelDiceScript[i].gameObject.activeSelf)
                lastDiceSprite[i] = panelDiceScript[i].GetCurrentSprite();
        }
        return lastDiceSprite;
    }
}
