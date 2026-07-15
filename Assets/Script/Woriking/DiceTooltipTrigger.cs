using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SortingGroup))]
public class DiceTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private DiceData diceData;

    private SortingGroup _sortingGroup;
    private SpriteRenderer _raycastRenderer;

    private void Awake()
    {
        _sortingGroup = GetComponent<SortingGroup>();

        _raycastRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (_raycastRenderer == null)
            _raycastRenderer = gameObject.AddComponent<SpriteRenderer>();

        _raycastRenderer.sprite = null;
        _raycastRenderer.color = new Color(1f, 1f, 1f, 0f);

        SyncRaycastSorting();
    }

    public void SyncRaycastSorting()
    {
        if (_raycastRenderer == null || _sortingGroup == null) return;
        _raycastRenderer.sortingLayerID = _sortingGroup.sortingLayerID;
        _raycastRenderer.sortingOrder = _sortingGroup.sortingOrder;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (diceData == null) return;
        GameDiceTooltipController.instance.Show(diceData.abilityName, diceData.Desc, transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameDiceTooltipController.instance.Hide();
    }
}