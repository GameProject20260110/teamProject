using UnityEngine;
using System.Collections.Generic;

public class MapSaveLoad : MonoBehaviour
{
    public static MapSaveLoad instance;

    private const string MAP_SAVE_DATA = "mapdata.json";

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(instance);
    }

    public bool HasSaveData()
    {
        return SaveManager.instance != null && SaveManager.instance.HasSaveFile(MAP_SAVE_DATA);
    }

    public void Save(List<MapNodeData> nodes, List<int> visitedNodeIds, int currentLayer, int previousNodeId, List<PathLineData> pathLines)
    {
        MapSaveData saveData = new MapSaveData
        {
            nodes = nodes,
            visitedNodeIds = visitedNodeIds,
            currentLayer = currentLayer,
            previousNodeId = previousNodeId,
            pathLines = pathLines
        };

        SaveManager.instance?.Save(saveData, MAP_SAVE_DATA);
    }

    public MapSaveData Load()
    {
        return SaveManager.instance?.Load<MapSaveData>(MAP_SAVE_DATA);
    }

    public void Delete()
    {
        SaveManager.instance?.Delete(MAP_SAVE_DATA);
    }
}
