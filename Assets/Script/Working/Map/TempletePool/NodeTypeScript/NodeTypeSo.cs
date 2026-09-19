using UnityEngine;

public abstract class NodeTypeData : ScriptableObject
{
    [SerializeField] private MNodeType type;
    [SerializeField] private Sprite icon;

    public MNodeType Type => type;
    public Sprite Icon => icon;
}
