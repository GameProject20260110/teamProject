using UnityEngine;
using UnityEngine.EventSystems;

public class DiceTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private DiceData diceData;
    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameDiceTooltipController.instance.Show(diceData.abilityName, diceData.Desc, rect);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameDiceTooltipController.instance.Hide();
    }
}
