using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapNode : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("노드 설정")]
    [SerializeField] private NodeType nodeType;
    [SerializeField] private SpriteRenderer icon;

    [Header("노드 타입별 아이콘")]
    [SerializeField] private Sprite battleSprite;
    [SerializeField] private Sprite shopSprite;
    [SerializeField] private Sprite eventSprite;
    [SerializeField] private Sprite bossSprite;

    [Header("상태")]
    [SerializeField] private bool isVisited = false;
    [SerializeField] private bool isSelectable = false;

    private Vector2 OriginScale;
    public int NodeId { get; private set; }
    public bool IsVisited => isVisited;

    public void Initialize(int id, NodeType type, bool selectable)
    {
        OriginScale = transform.localScale;
        NodeId = id;
        nodeType = type;
        isSelectable = selectable;
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (icon == null) return;

        switch(nodeType)
        {
            case NodeType.Battle: icon.sprite = battleSprite; break;
            case NodeType.Shop: icon.sprite = shopSprite; break;
            case NodeType.Random: icon.sprite = eventSprite; break;
            case NodeType.Boss: icon.sprite = bossSprite; break;
        }
        icon.color = isVisited ? new Color(1, 1, 1, 0.4f) : Color.white;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isSelectable || isVisited) return;
        if (MapCameraController.instance.IsDragging) return;
        AudioManager.instance.PlaySfx("MapNode");
        MapManager.instance?.OnNodeSelected(this);

    }

    public NodeType GetNodeType() => nodeType;

    public void SetVisited()
    {
        isVisited = true;
        UpdateIcon();
    }

    public void SetSelectable(bool selectable)
    {
        isSelectable = selectable;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(OriginScale * 1.3f, 0.2f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(OriginScale, 0.3f).SetEase(Ease.OutQuad);
    }
}
