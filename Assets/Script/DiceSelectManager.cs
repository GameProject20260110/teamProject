using UnityEngine;
using UnityEngine.UI;

public class DiceSelectManager : MonoBehaviour
{
    public static DiceSelectManager instance;
    public GameObject Synergy;
    Transform synergyDice;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        for (int i = 0; i < Player.instance.player.DiceSo.Length; i++)
        {
            if (Player.instance.player.DiceSo[i] != null)
            {
                PushSynergyDice(Player.instance.player.DiceSo[i]);
            }
        }
    }

    public void PushSynergyDice(DiceData Dice)
    {
        for (int i = 0; i < Synergy.transform.childCount; i++)
        {
            synergyDice = Synergy.transform.GetChild(i);
            if (!synergyDice.gameObject.activeSelf)
            {
                synergyDice.GetComponent<SynergyDice>().UpdateDiceInfo(Dice);
                synergyDice.gameObject.SetActive(true);
                return;
            }
        }
    }

    public void PopSynergyDice(DiceData Dice)
    {
        for (int i = 0; i < Synergy.transform.childCount; i++)
        {
            synergyDice = Synergy.transform.GetChild(i);
            if (synergyDice.gameObject.activeSelf &&
                synergyDice.GetComponent<SynergyDice>().Dice == Dice)
            {
                synergyDice.gameObject.SetActive(false);
                return;
            }
        }
    }

    public void SelectDiceComb()
    {
        for(int i = 0;i < Synergy.transform.childCount; i++)
        {
            synergyDice = Synergy.transform.GetChild(i);
            if (!synergyDice.gameObject.activeSelf)
            {
                Player.instance.PushPlayerDices(Player.instance.defaultDice,i);
                synergyDice.GetComponent<SynergyDice>().UpdateDiceInfo(Player.instance.defaultDice);
                synergyDice.gameObject.SetActive(true);
            }
        }
    }
}
