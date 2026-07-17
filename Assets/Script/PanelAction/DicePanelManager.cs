using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DicePanelManager : MonoBehaviour
{
    public static DicePanelManager instance;
    public enum PanelType
    {
        DicePanel,
        AttackPanel,
        DefensePanel
    }

    [Header("패널")]
    public AttackPanelUI attackPanel;
    public AttackPanelUI attackEnemyPanel;
    public DefensePanelUI defensePanel;
    public DefensePanelUI defenseEnemyPanel;

    [Header("배경 판때기")]
    public Transform attackPanelBg;
    public Transform defensePanelBg;
    public Transform dicePanelBg;

    private Dictionary<Dice, PanelType> _diceLocation = new Dictionary<Dice, PanelType>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void OnDicePickUp(Dice dice)
    {
        if (!_diceLocation.ContainsKey(dice)) return;

        PanelType currentPanel = _diceLocation[dice];
        if (currentPanel == PanelType.AttackPanel)
            attackPanel.RemoveDice(dice);
        if(currentPanel == PanelType.DefensePanel)
            defensePanel.RemoveDice(dice);

        _diceLocation.Remove(dice);
    }

    public bool OnDiceDrop(Dice dice, PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach(var result in results)
            Debug.Log($"감지된 오브젝트 : {result.gameObject.name}");

        foreach(var result in results)
        {
            if(attackPanelBg != null && IsChildOf(result.gameObject, attackPanelBg?.gameObject)) 
            {
                if(attackPanel.TryPlaceDice(dice))
                {
                    _diceLocation[dice] = PanelType.AttackPanel;
                    return true;
                }
            }

            if (defensePanelBg != null && IsChildOf(result.gameObject, defensePanelBg?.gameObject))
            {
                if (defensePanel.TryPlaceDice(dice))
                {
                    _diceLocation[dice] = PanelType.DefensePanel;
                    return true;
                }
            }

            if (dicePanelBg != null && IsChildOf(result.gameObject, dicePanelBg?.gameObject))
            {
                dice.GetComponent<DraggableDice>()?.ReturnToOriginalSlot();
                _diceLocation[dice] = PanelType.DicePanel;
                return true;
            }
        }
        return false;
    }

    public void RestoreDiceLocation(Dice dice, Transform originalParent)
    {
        if(originalParent.IsChildOf(attackPanel.transform))
        {
            attackPanel.TryPlaceDice(dice);
            _diceLocation[dice] = PanelType.AttackPanel;
        }
        else if (originalParent.IsChildOf(defensePanel.transform))
        {
            defensePanel.TryPlaceDice(dice);
            _diceLocation[dice] = PanelType.DefensePanel;
        }
        else
        {
            _diceLocation[dice] = PanelType.DicePanel;
        }

    }

    public bool HasAnyDiceInPanel() 
    {
        return attackPanel.GetDiceCount() > 0 || defensePanel.GetDiceCount() > 0;
    }

    public void ResetAllDice(Dice[] allDice)
    {
        foreach(var dice in allDice)
        {
            if (dice == null) continue;

            dice.transform.SetParent(dice.OriginalSlot, false);
            dice.transform.localPosition = Vector3.zero;
            dice.transform.localScale = Vector3.one;

            dice.SetResult(1);
        }
        Clear();
    }

    private bool IsChildOf(GameObject obj, GameObject parent)
    {
        if (obj == null || parent == null) return false;
        return obj == parent || obj.transform.IsChildOf(parent.transform);
    }

    public void Clear()
    {
        attackPanel.Clear();
        defensePanel.Clear();
        _diceLocation.Clear();
    }
}

