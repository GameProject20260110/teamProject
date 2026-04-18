using Cysharp.Threading.Tasks;
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

    public bool isRolling => diceRoller != null && diceRoller.isRolling;
    void Start()
    {
        if(GameManager.instance != null) GameManager.instance.diceManager = this;

        SetupDiceBoard();
    }

    public void SetupDiceBoard()
    {
        diceSetup.Setup(panelDiceScript, defaultDice);
    }
    public async UniTask<Dice[]> StartRolling()
    {
        Dice[] allDice = GetAllDice();
        await diceRoller.StartRoll(allDice, rollArea);
        return allDice;
    }

    public Dice[] GetAllDice()
    {
        Dice[] allDice = new Dice[panelDiceScript.Length ];
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
