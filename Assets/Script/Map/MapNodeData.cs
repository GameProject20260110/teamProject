using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapNodeData
{
    public int id;
    public NodeType nodeType;
    public int layer;
    public int xIndex;
    public List<int> nextNodeIDs = new List<int>();
}
