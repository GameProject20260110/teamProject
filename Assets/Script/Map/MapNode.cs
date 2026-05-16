using UnityEngine;
using UnityEngine.EventSystems;

public class MapNode : MonoBehaviour, IPointerClickHandler
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

    public int NodeId { get; private set; }

    private void Start()
    {
        UpdateIcon();
    }

    public void Initialize(int id, NodeType type, bool selectable)
    {
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
            case NodeType.Event: icon.sprite = eventSprite; break;
            case NodeType.Boss: icon.sprite = bossSprite; break;
        }

        icon.color = isVisited ? new Color(1, 1, 1, 0.4f) : Color.white;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isSelectable || isVisited) return;
        if (MapCameraController.instance.IsDragging) return;
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

}
