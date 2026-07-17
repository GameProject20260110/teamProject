using UnityEngine;
using UnityEngine.EventSystems;

public class DiceTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private DiceData diceData;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"{name} È£¹öµÊ / diceData: {(diceData != null ? diceData.name : "NULL")}");
        if (diceData == null) return;
        GameDiceTooltipController.instance.Show(diceData.abilityName, diceData.Desc, transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameDiceTooltipController.instance.Hide();
    }
}
