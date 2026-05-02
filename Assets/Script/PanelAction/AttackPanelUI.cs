using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class AttackPanelUI : MonoBehaviour
{
    public TextMeshProUGUI attackValue;
    public Transform diceContainer;

    private List<Dice> _placedDices = new List<Dice>();
    private int _cuurentAttackValue = 0;

    public int GetTotal() => _cuurentAttackValue;
    public List<Dice> GetDices() => _placedDices;

    public bool TryPlaceDice(Dice dice)
    {
        if (_placedDices.Count >= 6) return false;
        if (_placedDices.Contains(dice)) return false;

        _placedDices.Add(dice);
        dice.transform.SetParent(diceContainer);
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
    }

    public void Clear()
    {
        _placedDices.Clear();
        _cuurentAttackValue = 0;
        attackValue.text = "0";
    }
}
