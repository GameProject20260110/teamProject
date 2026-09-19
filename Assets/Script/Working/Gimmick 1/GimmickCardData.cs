using UnityEngine;

[CreateAssetMenu(fileName = "GimmickCard_", menuName = "Battle/Gimmick Card")]
public class GimmickCardData : ScriptableObject
{
    [SerializeField] private string gimmickId;
    [SerializeField] private string displayName;
    [TextArea][SerializeField] private string description;
    [SerializeField] private Sprite frontSprite;

    public string GimmickId => gimmickId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite FrontSprite => frontSprite;
}