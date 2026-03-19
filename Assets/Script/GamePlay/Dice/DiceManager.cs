using UnityEngine;

public class DiceManager : MonoBehaviour
{
  
    [Header("UI 연결")]
    public RectTransform rollArea;

    [Header("주사위 오브젝트")]
    public Dice[] panelDiceScript;

    [Header("기본 설정")]
    public DiceData defaultDice;

    [Header("모듈")]
    public DiceSetup diceSetup;
    public DiceRoller diceRoller;
    public WizardHandler wizardHandler;

    public bool isRolling => diceRoller != null && diceRoller.isRolling;
    void Start()
    {
        if(GameManager.instance != null) GameManager.instance.diceManager = this;

        SetupDiceBoard();
    }

    public void SetupDiceBoard()
    {
        diceSetup.Setup(panelDiceScript, defaultDice);
        wizardHandler.Setup(panelDiceScript, rollArea);
    }
    public void StartRolling()
    {
        Dice[] allDice = GetAllDice();
        diceRoller.OnRollComplete = () => GameManager.instance.OnDiceRollComplete(allDice);
        diceRoller.StartRoll(allDice, rollArea);
    }

    public Dice[] GetAllDice()
    {
        var wizardSlots = wizardHandler.GetWizardDice();
        Dice[] allDice = new Dice[panelDiceScript.Length + wizardSlots.Count];
        panelDiceScript.CopyTo(allDice, 0);
        for(int i = 0; i < wizardSlots.Count; i++)
        {
            allDice[panelDiceScript.Length + i] = wizardSlots[i];
        }
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
