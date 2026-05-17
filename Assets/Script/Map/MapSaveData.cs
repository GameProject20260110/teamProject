using System.Collections.Generic;

[System.Serializable]
public class PathLineData 
{
    public int fromNodeId;
    public int toNodeId;
}

[System.Serializable]
public class MapSaveData
{
    public List<MapNodeData> nodes = new List<MapNodeData>();
    public List<int> visitedNodeIds = new List<int>();
    public int currentLayer;
    public int previousNodeId = -1;
    public List<PathLineData> pathLines = new List<PathLineData>();
}
