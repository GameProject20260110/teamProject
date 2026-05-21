using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;
    [Header("맵 데이터")]
    [SerializeField] private MapDataSo mapData;

    [Header("프리팹")]
    [SerializeField] private MapNode nodePrefab;

    [Header("컨테이너")]
    [SerializeField] private List<Transform> layerContainers;
    [SerializeField] private Transform lineContainer;

    private List<MapNodeData> _generateNodes = new List<MapNodeData>();
    private List<MapNode> _spawnNode = new List<MapNode>();
    private int _currentLayer = 0;
    private int _previousNodeId = -1;
    private List<PathLineData> _pathLines = new List<PathLineData>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (MapSaveLoad.instance != null && MapSaveLoad.instance.HasSaveData())
            LoadMap();
        else
            GenerateMap();
    }

    public void GenerateMap()
    {
        if (mapData == null || layerContainers == null) return;

        List<List<Transform>> slotLayers = new List<List<Transform>>();
        foreach(var layerContainer in layerContainers)
        {
            List<Transform> slots = new List<Transform>();
            foreach (Transform slot in layerContainer)
                slots.Add(slot);
            slotLayers.Add(slots);
        }

        _generateNodes = mapData.GenerateMapNodes(slotLayers);

        int nodeIndex = 0;
        for(int i = 0; i < slotLayers.Count; i++)
        {
            for(int j = 0; j < slotLayers[i].Count; j++)
            {
                if (nodeIndex >= _generateNodes.Count) break;

                MapNodeData nodeData = _generateNodes[nodeIndex];
                Transform slot = slotLayers[i][j];

                MapNode node = Instantiate(nodePrefab, slot.position, Quaternion.identity, slot);
                node.Initialize(nodeData.id, nodeData.nodeType, nodeData.layer == 0);
                node.name = $"Node_{nodeData.id}";
                nodeIndex++;
                _spawnNode.Add(node);
                Debug.Log($"노드 생성: {node.name}, id: {nodeData.id}");
            }
        }
        MapPathDrawer.instance?.Initialize(lineContainer);
        MapPathDrawer.instance?.DrawLines(_generateNodes, _spawnNode);

        
        if(layerContainers.Count > 0)
        {
            float startY = layerContainers[_currentLayer].position.y;
            MapCameraController.instance?.MoveToLayerImmediate(startY);
        }
    }

    private void LoadMap()
    {
        MapSaveData saveData = MapSaveLoad.instance.Load();
        if(saveData == null)
        {
            GenerateMap();
            return;
        }

        _generateNodes = saveData.nodes;
        _currentLayer = saveData.currentLayer;
        _previousNodeId = saveData.previousNodeId;
        _pathLines = saveData.pathLines;

        List<List<Transform>> slotLayers = ColletSlotLayers();
        SpawnNodes(slotLayers);

        foreach(var visitedId in saveData.visitedNodeIds)
        {
            MapNode visitedNode = _spawnNode.Find(n => n.NodeId == visitedId);
            visitedNode?.SetVisited();
        }

        List<MapNodeData> currentLayerNodes = _generateNodes.FindAll(n => n.layer == _currentLayer);
        foreach(var nodeData in currentLayerNodes)
        {
            MapNode node = _spawnNode.Find(n => n.NodeId == nodeData.id);
            node?.SetSelectable(true);
        }

        MapPathDrawer.instance?.Initialize(lineContainer);
        MapPathDrawer.instance?.DrawLines(_generateNodes, _spawnNode);

        foreach(var pathLine in _pathLines)
        {
            MapPathDrawer.instance?.RestorePathLine(pathLine.fromNodeId, pathLine.toNodeId, _spawnNode);
        }

        if(layerContainers.Count > 0 && _currentLayer < layerContainers.Count)
        {
            float startY = layerContainers[_currentLayer].position.y;
            MapCameraController.instance?.MoveToLayerImmediate(startY);
        }
    }

    private List<List<Transform>> ColletSlotLayers()
    {
        List<List<Transform>> slotLayers = new List<List<Transform>>();
        foreach(var layerContainer in layerContainers)
        {
            List<Transform> slots = new List<Transform>();
            foreach (Transform slot in layerContainer)
                slots.Add(slot);
            slotLayers.Add(slots);
        }
        return slotLayers;
    }

    private void SpawnNodes(List<List<Transform>> slotLayers)
    {
        int nodeIndex = 0;
        for(int i = 0; i < slotLayers.Count; i++)
        {
            for(int j = 0; j < slotLayers[i].Count; j++)
            {
                if (nodeIndex >= _generateNodes.Count) break;

                MapNodeData nodeData = _generateNodes[nodeIndex];
                Transform slot = slotLayers[i][j];

                MapNode node = Instantiate(nodePrefab, slot.position, Quaternion.identity);
                node.Initialize(nodeData.id, nodeData.nodeType, nodeData.layer == 0);
                node.name = $"Node_{nodeData.id}";
                nodeIndex++;
                _spawnNode.Add(node);
            }
        }
    }

    public async void OnNodeSelected(MapNode node)
    { 
        MapNodeData nodeData = _generateNodes.Find(n => n.id == node.NodeId);
        if (nodeData == null) return;

        node.SetVisited();
        SetNextLayerSelectable(nodeData);

        if (_previousNodeId >= 0 && MapPathDrawer.instance != null)
        {
            await MapPathDrawer.instance.DrawPathLineAnimated(_previousNodeId, nodeData.id, _spawnNode);
            _pathLines.Add(new PathLineData
            {
                fromNodeId = _previousNodeId,
                toNodeId = nodeData.id
            });
        }
            

        _previousNodeId = nodeData.id;
        _currentLayer++;

        List<int> visitedIds = _spawnNode.FindAll(n => n.IsVisited).ConvertAll(n => n.NodeId);
        MapSaveLoad.instance?.Save(_generateNodes, visitedIds, _currentLayer, _previousNodeId, _pathLines);

        MoveCameraToLayer(_currentLayer);
        HandleNodeType(nodeData.nodeType);
    }

    private void SetNextLayerSelectable(MapNodeData selectedNode)
    {
        foreach(var nextId in selectedNode.nextNodeIDs)
        {
            MapNode nextNode = _spawnNode.Find(n => n.NodeId == nextId);
            nextNode?.SetSelectable(true);
        }
    }

    private void MoveCameraToLayer(int layer)
    {
        if (layer >= layerContainers.Count) return;
        float targetY = layerContainers[layer].position.y;
        MapCameraController.instance?.MoveToLayer(targetY);
    }

    private void HandleNodeType(NodeType nodeType)
    {
        switch(nodeType)
        {
            case NodeType.Battle:
                MapNodeData selectedNode = _generateNodes.Find(n => n.id == _previousNodeId);
                if(selectedNode != null)
                {
                    EnemyData enemy = null;
                    if(selectedNode.layer == 0)
                    {
                        var enemies = mapData.startLayerEnemies;
                        if(enemies != null && enemies.Count > selectedNode.enemyDataIndex)
                            enemy = enemies[selectedNode.enemyDataIndex];
                    }
                    else
                    {
                        int layerIndex = selectedNode.layer - 1;
                        if (layerIndex >= 0 && layerIndex < mapData.layers.Count)
                        {
                            var enemies = mapData.layers[layerIndex].enemies;
                            if (enemies != null && enemies.Count > selectedNode.enemyDataIndex)
                                enemy = enemies[selectedNode.enemyDataIndex];
                        }
                    }
                    BattleDataManager.instance?.SetBattleData(enemy);
                }
                SceneController.instance?.LoadGameScene();
                break;
            case NodeType.Boss:
                BattleDataManager.instance?.SetBossBattleData(mapData.bossEnemyData);
                SceneController.instance?.LoadGameScene();
                break;
            case NodeType.Shop:
                break;
            case NodeType.Event:
                break;
        }
    }

    public void ClearMapSave()
    {
        MapSaveLoad.instance?.Delete();
        _pathLines.Clear();
        _currentLayer = 0;
        _previousNodeId = -1;
    }
}
