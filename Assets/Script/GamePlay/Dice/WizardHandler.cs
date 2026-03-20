using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class WizardHandler : MonoBehaviour
{
    public GameObject diceSlotPrefab;
    public GameObject scoreTextPrefab;
    public RectTransform scoreTextContainer;
    public DiceData wizardSummonDiceData;
    private List<Dice> _wizardDiceSlots = new List<Dice>();
    private List<GameObject> _scoreText = new List<GameObject>();

    public void Setup(Dice[] diceScript, RectTransform rollArea)
    {
        foreach(var slot in _wizardDiceSlots)
        {
            if (slot != null) Destroy(slot.transform.parent.gameObject);
        }
        _wizardDiceSlots.Clear();

        foreach(var text in _scoreText)
        {
            if (text != null) Destroy (text);
        }
        _scoreText.Clear();

        int wizardCount = 0;
        for(int i = 0; i < diceScript.Length; i++)
        {
            if (diceScript[i] == null || diceScript[i].MyState == null) continue;
            if (diceScript[i].MyState.diceData is WizardDiceAbility)
            {
                wizardCount++;
            }
        }
        if (wizardCount == 0) return;
        for(int i = 0; i < wizardCount;i++)
        {
            GameObject slotObj = Instantiate(diceSlotPrefab, rollArea);
            Dice dice = slotObj.GetComponentInChildren<Dice>();
            GameObject textObj = Instantiate(scoreTextPrefab, scoreTextContainer);
            dice.diceScoreText = textObj.GetComponent<TextMeshProUGUI>();
            _scoreText.Add(textObj);

            dice.Initialize(diceScript.Length + i, wizardSummonDiceData);
            dice.MyState.isForceOdd = true;
            _wizardDiceSlots.Add(dice);
        }
    }

    public List<Dice> GetWizardDice()
    {
        return _wizardDiceSlots;
    }
}
