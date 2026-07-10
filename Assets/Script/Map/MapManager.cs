using UnityEngine;
using System.Collections.Generic;
using VContainer;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    [Header("맵 데이터")]
    [SerializeField] private MapDataSo mapData;
    [SerializeField] private MapNode nodePrefab;
    [SerializeField] private List<Transform> layerContainers;
    [SerializeField] private Transform lineContainer;

    private List<MapNodeData> _generateNodes = new List<MapNodeData>();
    private List<MapNode> _spawnNode = new List<MapNode>();
    private int _currentLayer = 0;
    private int _previousNodeId = -1;
    private List<PathLineData> _pathLines = new List<PathLineData>();

    [SerializeField] private string MapBgmKey;

    private BattleDataManager _battleDataManager;
    private AudioManager _audioManager;
    private SceneController _sceneController;
    private MapPathDrawer _mapPathDrawer;
    private MapSaveLoad _mapSaveLoad;
    private MapCameraController _mapCameraController;
    private MapIntroController _mapIntroController;

    [Inject]
    public void Construct(AudioManager audioManager, BattleDataManager battleDataManager, SceneController sceneController,
        MapPathDrawer mapPathDrawer, MapSaveLoad mapSaveLoad, MapCameraController mapCameraController, MapIntroController mapIntroController)
    {
        _battleDataManager = battleDataManager;
        _audioManager = audioManager;
        _sceneController = sceneController;
        _mapPathDrawer = mapPathDrawer;
        _mapSaveLoad = mapSaveLoad;
        _mapCameraController = mapCameraController;
        _mapIntroController = mapIntroController;
        Instance = this;
    }

    private void Start()
    {
        if (_mapSaveLoad != null && _mapSaveLoad.HasSaveData())
            LoadMap();
        else
            GenerateMap();

        if (_mapCameraController != null)
            _mapCameraController.ResetZoom();

        if (_sceneController.isFirstEntry)
        {   
            SetLineVisible(false);
            _mapIntroController.PlayIntro();            
        }
        else
        {
            _mapIntroController.SkipIntro();
            SetLineVisible(true);
            MoveCameraToLayer(_currentLayer);
            _audioManager.PlayBgm(MapBgmKey);
        }

        if (MainOption.instance != null)
            MainOption.instance.SetSettingsButtonActive(true);
    }

    public void StartIntro()
    {
        MoveCameraToLayer(_currentLayer);
        _audioManager.PlayBgm(MapBgmKey);
        SetLineVisible(true);
    }

    private void SetLineVisible(bool visible)
    {
        foreach (Transform line in lineContainer)
        {
            line.gameObject.SetActive(visible);
        }
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
        _mapPathDrawer?.Initialize(lineContainer);
        _mapPathDrawer?.DrawLines(_generateNodes, _spawnNode);

        
        if(layerContainers.Count > 0)
        {
            float startY = layerContainers[_currentLayer].position.y;
            _mapCameraController?.MoveToLayerImmediate(startY);
        }
    }

    private void LoadMap()
    {
        MapSaveData saveData = _mapSaveLoad.Load();
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

        _mapPathDrawer?.Initialize(lineContainer);
        _mapPathDrawer?.DrawLines(_generateNodes, _spawnNode);

        foreach(var pathLine in _pathLines)
        {
            _mapPathDrawer?.RestorePathLine(pathLine.fromNodeId, pathLine.toNodeId, _spawnNode);
        }

        if(layerContainers.Count > 0 && _currentLayer < layerContainers.Count)
        {
            float startY = layerContainers[_currentLayer].position.y;
            _mapCameraController?.MoveToLayerImmediate(startY);
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

        if (_previousNodeId >= 0 && _mapPathDrawer != null)
        {
            await _mapPathDrawer.DrawPathLineAnimated(_previousNodeId, nodeData.id, _spawnNode);
            _pathLines.Add(new PathLineData
            {
                fromNodeId = _previousNodeId,
                toNodeId = nodeData.id
            });
        }
            

        _previousNodeId = nodeData.id;
        _currentLayer++;

        List<int> visitedIds = _spawnNode.FindAll(n => n.IsVisited).ConvertAll(n => n.NodeId);
        _mapSaveLoad?.Save(_generateNodes, visitedIds, _currentLayer, _previousNodeId, _pathLines);

        await _mapCameraController.ZoomToNode(node.transform.position);
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
        _mapCameraController?.MoveToLayer(targetY);
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

                    if(enemy == null)
                    {
                        Debug.LogWarning("적 데이터가 없습니다.");
                        return;
                    }
                    _battleDataManager?.SetBattleData(enemy);
                }
                _sceneController?.LoadGameScene();
                break;
            case NodeType.Boss:
                _battleDataManager?.SetBossBattleData(mapData.bossEnemyData);
                _sceneController?.LoadBossScene();
                break;
            case NodeType.Shop:
                _sceneController.LoadShopScene();
                break;
            case NodeType.Random:
                MapNodeData randomNodeData = _generateNodes.Find(n => n.id == _previousNodeId);
                if(randomNodeData != null)
                {
                    int layerIndex = randomNodeData.layer - 1;
                    if(layerIndex >= 0 && layerIndex < mapData.layers.Count)
                    {
                        var weights = mapData.layers[layerIndex].randomNodeWeight;
                        NodeType randomType = mapData.GetRandomNodeType(weights);
                        HandleNodeType(randomType);
                        return;
                    }
                }
                break;
            case NodeType.Event:
                _sceneController?.LoadEventScene();
                break;
        }
    }
    
    public void ClearMapSave()
    {
        _mapSaveLoad?.Delete();
        _pathLines.Clear();
        _currentLayer = 0;
        _previousNodeId = -1;
    }
}
