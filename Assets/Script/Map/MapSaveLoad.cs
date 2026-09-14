using System.Collections.Generic;
using VContainer.Unity;
using UnityEngine;

public class MapSaveLoad : IInitializable
{
    public static MapSaveLoad Instance { get; private set; }

    private const string MAP_SAVE_DATA = "mapdata.json";
    private readonly SaveManager _saveManager;

    public MapSaveLoad(SaveManager saveManager)
    {
        _saveManager = saveManager;
    }

    public void Initialize()
    {
        Instance = this;
    }

    public bool HasSaveData() => _saveManager != null && _saveManager.HasSaveFile(MAP_SAVE_DATA);

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
        _saveManager?.Save(saveData, MAP_SAVE_DATA);
    }

    public MapSaveData Load()
    {
        if (_saveManager == null) return null;

        if(!_saveManager.Load(MAP_SAVE_DATA, out MapSaveData data))
        {
            Debug.LogWarning("[MapSaveLoad] 맵 진행 상황을 불러오지 못했습니다. 새 맵으로 시작합니다.");
            return null;
        }

        return data;
    }
    public void Delete() => _saveManager?.Delete(MAP_SAVE_DATA);
}
