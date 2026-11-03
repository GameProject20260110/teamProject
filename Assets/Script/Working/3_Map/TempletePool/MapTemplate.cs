using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapNodeEntry
{
    public int id;
    public NodeTypeData typeData;
    public Vector2 position;
    public List<int> connectedNodeIds = new();
    public bool isStartNode;
    public NodeState initialState = NodeState.Locked;
}

[CreateAssetMenu(fileName = "MapTemplate", menuName = "MapSystem/Map Template")]
public class MapTemplate : ScriptableObject
{
    [SerializeField] private int stageIndex;
    [SerializeField] private List<MapNodeEntry> nodes = new();

    public int StageIndex => stageIndex;
    public IReadOnlyList<MapNodeEntry> Nodes => nodes;

#if UNITY_EDITOR
    public void EditorSetData(int stageIndex, List<MapNodeEntry> nodes)
    {
        this.stageIndex = stageIndex;
        this.nodes = nodes;
    }
#endif

    public MapNodeEntry FindNode(int id)
    {
        foreach (var node in nodes)
        {
            if (node.id == id) return node;
        }
        return null;
    }

#if UNITY_EDITOR
    public List<string> Validate()
    {
        var errors = new List<string>();

        int startCount = 0;
        var idSet = new HashSet<int>();

        foreach (var node in nodes)
        {
            if (!idSet.Add(node.id))
                errors.Add($"중복된 노드 id: {node.id}");

            if (node.isStartNode)
                startCount++;
        }

        if (startCount == 0)
            errors.Add("시작 노드(isStartNode)가 하나도 없습니다.");
        else if (startCount > 1)
            errors.Add($"시작 노드가 {startCount}개입니다. 1개여야 합니다.");

        foreach (var node in nodes)
        {
            foreach (var connectedId in node.connectedNodeIds)
            {
                if (!idSet.Contains(connectedId))
                    errors.Add($"노드 {node.id}가 존재하지 않는 노드 {connectedId}를 참조합니다.");
            }
        }

        // 고립 노드 체크: 시작 노드에서 BFS로 전부 도달 가능한지
        if (startCount == 1)
        {
            var start = nodes.Find(n => n.isStartNode);
            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(start.id);
            visited.Add(start.id);

            while (queue.Count > 0)
            {
                var current = FindNode(queue.Dequeue());
                if (current == null) continue;

                foreach (var neighborId in current.connectedNodeIds)
                {
                    if (visited.Add(neighborId))
                        queue.Enqueue(neighborId);
                }
            }

            foreach (var node in nodes)
            {
                if (!visited.Contains(node.id))
                    errors.Add($"노드 {node.id}는 시작 노드에서 도달할 수 없습니다 (고립됨).");
            }
        }

        return errors;
    }
#endif
}