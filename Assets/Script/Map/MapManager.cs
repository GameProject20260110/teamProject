using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

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

    [Header("라인 설정")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.15f;
    [SerializeField] private Color lineColor = Color.black;

    private List<MapNodeData> _generateNodes = new List<MapNodeData>();
    private List<MapNode> _spawnNode = new List<MapNode>();
    private int _currentLayer = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
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
        DrawLines();
        
        if(layerContainers.Count > 0)
        {
            float startY = layerContainers[_currentLayer].position.y;
            MapCameraController.instance?.MoveToLayerImmediate(startY);
        }
    }

    public void OnNodeSelected(MapNode node)
    { 
        MapNodeData nodeData = _generateNodes.Find(n => n.id == node.NodeId);
        if (nodeData == null) return;

        node.SetVisited();
        SetNextLayerSelectable(nodeData);

        _currentLayer++;
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
            case NodeType.Boss:
                SceneController.instance?.LoadGameScene();
                break;
            case NodeType.Shop:
                break;
            case NodeType.Event:
                break;
        }
    }

    private void DrawLines()
    {
        foreach(var nodeData in _generateNodes)
        {
            foreach(var nextId in nodeData.nextNodeIDs)
            {
                MapNode fromNode = _spawnNode.Find(n => n.NodeId == nodeData.id);
                MapNode toNode = _spawnNode.Find(n => n.NodeId == nextId);

                if (fromNode == null || toNode == null) continue;

                GameObject lineObj = new GameObject($"Line_{nodeData.id}_{nextId}");
                lineObj.transform.SetParent(lineContainer);

                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.SetPosition(0, fromNode.transform.position);
                lr.SetPosition(1, toNode.transform.position);
                lr.startWidth = lineWidth;
                lr.endWidth = lineWidth;
                lr.material = lineMaterial;
                lr.material.mainTextureScale = new Vector2(3f, 1f);
                lr.startColor= lineColor; 
                lr.endColor = lineColor;
                lr.textureMode = LineTextureMode.Tile;
                Debug.Log($"linewidth : {lineWidth}");
            }
        }
    }

}
