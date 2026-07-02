using TMPro;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class AttackPanelUI : MonoBehaviour
{
    public TextMeshProUGUI attackValue;
    public Transform[] slots;

    public List<Dice> _placedDices = new List<Dice>();
    private int _cuurentAttackValue = 0;

    public int GetTotal() => _cuurentAttackValue;
    public List<Dice> GetDices() => _placedDices;
    public int GetDiceCount() => _placedDices.Count;

    public bool TryPlaceDice(Dice dice)
    {
        Transform emptySlot = FindEmptySlot();
        if (emptySlot == null) return false;
        if (_placedDices.Contains(dice)) return false;

        _placedDices.Add(dice);
        dice.transform.SetParent(emptySlot, false);
        dice.transform.localPosition = Vector3.zero;
        dice.transform.localScale = Vector3.one;

        UpdateValue();
        return true;
    }

    public void RemoveDice(Dice dice)
    {
        if (!_placedDices.Contains(dice)) return;
        _placedDices.Remove(dice);
        UpdateValue();
    }

    private Transform FindEmptySlot()
    {
        foreach(var slot in slots)
        {
            if (slot.childCount == 0)
                return slot;
        }
        return null;
    }

    private void UpdateValue()
    {
        int newValue = 0;
        foreach(var dice in _placedDices)
        {
            newValue += dice.MyState.originalValue;
        }

        DOVirtual.Int(_cuurentAttackValue, newValue, 0.2f, (x) =>
        {
            attackValue.text = x.ToString();
        });
        _cuurentAttackValue = newValue;

        UpdateTurnEndButtonGlow();
    }

    private void UpdateTurnEndButtonGlow() 
    {
        bool hasAnyDice = DicePanelManager.instance?.HasAnyDiceInPanel() ?? false;
        if (hasAnyDice)
            UiController.instance?.ShowGlowImage();
        else 
            UiController.instance?.HideGlowImage();
    }

    public void Clear()
    {
        foreach (var slot in slots)
        {
            if (slot.childCount > 0)
            {
                var leftover = slot.GetChild(0);
                var leftoverDice = leftover.GetComponent<Dice>();
                if (leftoverDice != null && leftoverDice.OriginalSlot != null)
                    leftover.SetParent(leftoverDice.OriginalSlot, false);
            }
        }

        _placedDices.Clear();
        _cuurentAttackValue = 0;
        attackValue.text = "0";
    }
}
