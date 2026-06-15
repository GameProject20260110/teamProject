using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class MapPathDrawer : MonoBehaviour
{
    public static MapPathDrawer instance;

    [Header("라인 설정")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Color lineColor = Color.black;
    [SerializeField] private float pathAnimDuration = 0.5f;
    [SerializeField] private float nodeOffset = 0.5f;

    private Transform _lineContainer;
    private List<GameObject> _pathLines = new List<GameObject>();
    // 기존 라인 저장
    private Dictionary<string, GameObject> _lineObjects = new Dictionary<string, GameObject>();


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void Initialize(Transform lineContainer)
    {
        _lineContainer = lineContainer;
    }

    public void DrawLines(List<MapNodeData> generatedNodes, List<MapNode> spawnedNodes)
    {
        foreach(var nodeData in generatedNodes)
        {
            foreach (var nextId in nodeData.nextNodeIDs)
            {
                MapNode fromNode = spawnedNodes.Find(n => n.NodeId == nodeData.id);
                MapNode toNode = spawnedNodes.Find(n => n.NodeId == nextId);

                if (fromNode == null || toNode == null) continue;

                // offset적용
                Vector3 direction = (toNode.transform.position - fromNode.transform.position).normalized;
                Vector3 adjustedStart = fromNode.transform.position + direction * nodeOffset;
                Vector3 adjustedEnd = toNode.transform.position - direction * nodeOffset;

                GameObject lineObj = new GameObject($"Line_{nodeData.id}_{nextId}");
                lineObj.transform.SetParent(_lineContainer);

                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.SetPosition(0, adjustedStart);
                lr.SetPosition(1, adjustedEnd);
                lr.startWidth = lineWidth;
                lr.endWidth = lineWidth;
                lr.material = lineMaterial;
                lr.material.mainTextureScale = new Vector2(3f, 1f);
                lr.startColor = lineColor;
                lr.endColor = lineColor;
                lr.textureMode = LineTextureMode.Tile;

                _lineObjects[$"{nodeData.id}_{nextId}"] = lineObj;
            }
        }
    }

    public async UniTask DrawPathLineAnimated(int fromNodeId, int toNodeId, List<MapNode> spawnedNodes)
    {
        MapNode fromNode = spawnedNodes.Find(n => n.NodeId == fromNodeId);
        MapNode toNode = spawnedNodes.Find(n => n.NodeId == toNodeId);

        if (fromNode == null || toNode == null) return;

        Vector3 direction = (toNode.transform.position - fromNode.transform.position).normalized;
        Vector3 adjustStart = fromNode.transform.position + direction * nodeOffset;
        Vector3 adjustEnd = toNode.transform.position - direction * nodeOffset;

        await DrawSinglePathLine(adjustStart, adjustEnd, fromNodeId, toNodeId);
    }

    private async UniTask DrawSinglePathLine(Vector3 startPos, Vector3 endPos, int fromId, int toId)
    {
        GameObject pathObj = new GameObject($"Path_{fromId}_{toId}");
        pathObj.transform.SetParent(_lineContainer);

        LineRenderer lr = pathObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, startPos);
        lr.SetPosition(1, endPos);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.red;
        lr.endColor = Color.red;
        lr.sortingOrder = 1;

        _pathLines.Add(pathObj);

        float elapsed = 0f;
        while(elapsed < pathAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pathAnimDuration);
            lr.SetPosition(1, Vector3.Lerp(startPos, endPos, t));
            await UniTask.Yield();
        }

        lr.SetPosition(1, endPos);

        string key = $"{fromId}_{toId}";
        if(_lineObjects.TryGetValue(key, out GameObject lineObj))
        {
            Destroy(lineObj);
            _lineObjects.Remove(key);
        }
    }

    // 맵 로드 시 사용
    public void RestorePathLine(int fromNodeId, int toNodeId, List<MapNode> spawnedNodes)
    {
        MapNode fromNode = spawnedNodes.Find(n => n.NodeId == fromNodeId);
        MapNode toNode = spawnedNodes.Find(n => n.NodeId == toNodeId);

        if (fromNode == null && toNode == null) return;

        Vector3 direction = (toNode.transform.position - fromNode.transform.position).normalized;
        Vector3 adjustedStart = fromNode.transform.position + direction * nodeOffset;
        Vector3 adjustedEnd = toNode.transform.position - direction * nodeOffset;

        GameObject pathObj = new GameObject($"Path_{fromNodeId}_{toNodeId}");
        pathObj.transform.SetParent(_lineContainer);

        LineRenderer lr = pathObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, adjustedStart);
        lr.SetPosition(1, adjustedEnd);
        lr.startWidth = lineWidth * 1.5f;
        lr.endWidth = lineWidth * 1.5f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.red;
        lr.endColor = Color.red;
        lr.sortingOrder = 1;

        _pathLines.Add(pathObj);

        string key = $"{fromNodeId}_{toNodeId}";
        if (_lineObjects.TryGetValue(key, out GameObject lineObj))
        {
            Destroy(lineObj);
            _lineObjects.Remove(key);
        }
    }

    public void ClearPathLines()
    {
        foreach(var pathLine in _pathLines)
        {
            if (pathLine != null) Destroy(pathLine);
        }
        _pathLines.Clear();
    }
}
