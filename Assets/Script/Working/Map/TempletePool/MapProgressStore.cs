using System.Collections.Generic;
using System.Linq;

public class MapProgressStore
{
    public MapTemplate CurrentTemplate { get; private set; }
    public int CurrentNodeId { get; private set; }
    public IReadOnlyDictionary<int, NodeState> NodeStates => nodeStates;

    private Dictionary<int, NodeState> nodeStates = new();

    public bool HasProgress { get; private set; }

    public void Save(MapTemplate template, int currentNodeId, IEnumerable<NodeView> nodeViews)
    {
        CurrentTemplate = template;
        CurrentNodeId = currentNodeId;
        nodeStates = nodeViews.ToDictionary(v => v.NodeId, v => v.State);
        HasProgress = true;
    }

    public void Clear()
    {
        CurrentTemplate = null;
        CurrentNodeId = -1;
        nodeStates.Clear();
        HasProgress = false;
    }
}
