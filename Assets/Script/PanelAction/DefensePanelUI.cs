using TMPro;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class DefensePanelUI : MonoBehaviour
{
    public TextMeshProUGUI defenseValue;
    public Transform[] slots; 

    private int _currentDefenseValue = 0;
    private List<Dice> _placedDices = new List<Dice>();

    public int GetTotal() => _currentDefenseValue;
    public List<Dice> GetDices() => _placedDices;

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

    private Transform GetDiceSlot(Dice dice)
    {
        foreach(var slot in slots)
        {
            if (slot.childCount > 0 && slot.GetChild(0).GetComponent<Dice>() == dice)
                return slot;
        }
        return null;
    }

    private void UpdateValue()
    {
        int newValue = 0;
        foreach (var dice in _placedDices)
            newValue += dice.MyState.originalValue;

        DOVirtual.Int(_currentDefenseValue, newValue, 0.2f, (x) =>
        {
            defenseValue.text = x.ToString();
        });
        _currentDefenseValue = newValue;
    }

    public void Clear()
    {
        _placedDices.Clear();
        _currentDefenseValue = 0;
        defenseValue.text = "0";
    }
}
