using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SynergyDice : MonoBehaviour, IPointerClickHandler
{
    public DiceData Dice;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            if (gameObject.CompareTag("Scroll"))
            {
                //Player.instance.PushPlayerDices(Dice);
                PlayerManager.instance.PushPlayerDices(Dice);
                DiceSelectManager.instance.PushSynergyDice(Dice);
            }
            else if (gameObject.CompareTag("Synergy"))
            {
                //Player.instance.PullPlayerDices(Dice);
                PlayerManager.instance.PullPlayerDices(Dice);
                DiceSelectManager.instance.PopSynergyDice(Dice);
            }
        }
    }

    public void UpdateDiceInfo(DiceData Dice)
    {
        this.Dice = Dice;
        gameObject.GetComponent<Image>().sprite = Dice.skin.sprites[0];
        
    }
    

}
