using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeTypeWeight
{
    public NodeType nodeType;
    public int weight;
}

[System.Serializable]
public class LayerConfig
{
    public List<NodeTypeWeight> nodeTypeWeights;
    public List<EnemyData> enemies;
}

[CreateAssetMenu(fileName = "MapDataSo", menuName = "Stage/MapDataSo")]
public class MapDataSo : ScriptableObject
{
    [Header("1층 설정")]
    public List<EnemyData> startLayerEnemies;

    [Header("중간 레이어 설정")]
    public List<LayerConfig> layers= new List<LayerConfig>();

    [Header("보스 데이터 설정")]
    public BossDataSo bossEnemyData;

    public NodeType GetRandomNodeType(LayerConfig config)
    {
        int totalWeight = 0;
        foreach (var w in config.nodeTypeWeights)
            totalWeight += w.weight;

        int random = Random.Range(0, totalWeight);
        int current = 0;

        foreach(var w in config.nodeTypeWeights)
        {
            current += w.weight;
            if (random < current)
                return w.nodeType;
        }
        return NodeType.Battle;
    }

    public List<MapNodeData> GenerateMapNodes(List<List<Transform>> slotLayers)
    {
        List<MapNodeData> nodes = new List<MapNodeData>();
        int idCounter = 0;
        List<List<MapNodeData>> allLayers = new List<List<MapNodeData>>();

        // 1층 battle노드로 고정
        MapNodeData startNode = new MapNodeData
        {
            id = idCounter++,
            layer = 0,
            xIndex = 0,
            nodeType = NodeType.Battle,
            enemyDataIndex = GetRandomEnemyIndexFromIndex(startLayerEnemies)
        };
        nodes.Add(startNode);
        allLayers.Add(new List<MapNodeData> { startNode });

        // 중간 레이어
        for(int i = 1; i < slotLayers.Count - 1; i++)
        {
            LayerConfig config = i - 1 < layers.Count ? layers[i - 1] : layers[layers.Count - 1];
            List<MapNodeData> layerNodes = new List<MapNodeData>();
            
            for(int j = 0; j < slotLayers[i].Count; j++)
            {
                MapNodeData node = new MapNodeData
                {
                    id = idCounter++,
                    layer = i,
                    xIndex = j,
                    nodeType = GetRandomNodeType(config),
                    enemyDataIndex = GetRandomEnemyIndex(config)
                };
                layerNodes.Add(node);
                nodes.Add(node);
            }
            allLayers.Add(layerNodes);
        }

        // 보스레이어
        MapNodeData bossNode = new MapNodeData
        {
            id = idCounter++,
            layer = slotLayers.Count - 1,
            xIndex = 0,
            nodeType = NodeType.Boss
        };
        nodes.Add(bossNode);
        allLayers.Add(new List<MapNodeData> { bossNode });

        ConnectNodes(allLayers);
        return nodes;
    }

    private void ConnectNodes(List<List<MapNodeData>> allLayers)
    {
        int totalOneToTwo = 0;
        for(int i = 0; i < allLayers.Count - 1; i++)
        {
            List<MapNodeData> currentLayer = allLayers[i];
            List<MapNodeData> nextLayer = allLayers[i + 1];

            bool isBossLayer = (i == allLayers.Count - 2);

            if(isBossLayer)
            {
                foreach(var currentNode in currentLayer)
                {
                    if (!currentNode.nextNodeIDs.Contains(nextLayer[0].id))
                        currentNode.nextNodeIDs.Add(nextLayer[0].id);
                }
            }
            else
            {
                foreach(var nextNode in nextLayer)
                {
                    MapNodeData closest = GetClosesetNode(currentLayer, nextNode.xIndex);
                    if (!closest.nextNodeIDs.Contains(nextNode.id))
                        closest.nextNodeIDs.Add(nextNode.id);
                }

                foreach(var currentNode in currentLayer)
                {
                    if(currentNode.nextNodeIDs.Count == 0)
                    {
                        MapNodeData closest = GetClosesetNode(nextLayer, currentNode.xIndex);
                        currentNode.nextNodeIDs.Add(closest.id);
                    }

                    float oneToTwoChance = GetOneToTwoChance(totalOneToTwo);
                    if(Random.value < oneToTwoChance && currentNode.nextNodeIDs.Count < 2)
                    {
                        MapNodeData extra = GetExtraNode(nextLayer, currentNode.xIndex, currentNode.nextNodeIDs);
                        if(extra != null)
                        {
                            currentNode.nextNodeIDs.Add(extra.id);
                            totalOneToTwo++;
                        }
                    }
                }
            }
        }
    }

    private MapNodeData GetClosesetNode(List<MapNodeData> layer, int xIndex)
    {
        MapNodeData closest = null;
        int minDist = int.MaxValue;

        foreach(var node in layer)
        {
            int dist = Mathf.Abs(node.xIndex - xIndex);
            if(dist <= 1 && dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }

        if(closest == null)
        {
            minDist = int.MaxValue;
            foreach(var node in layer)
            {
                int dist = Mathf.Abs(node.xIndex - xIndex);
                if(dist < minDist)
                {
                    minDist = dist;
                    closest = node;
                }
            }
        }
        return closest;
    }

    private MapNodeData GetExtraNode(List<MapNodeData> nextLayer, int xIndex, List<int> alreadyConnected)
    {
        List<MapNodeData> candidates = new List<MapNodeData>();

        foreach(var node in nextLayer)
        {
            int diff = Mathf.Abs(node.xIndex - xIndex);
            if(diff <= 1 && !alreadyConnected.Contains(node.id))
            {
                candidates.Add(node);
            }
        }
        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    private float GetOneToTwoChance(int totalOneToTwo)
    {
        if (totalOneToTwo == 0) return 0.65f;
        if (totalOneToTwo == 1) return 0.2f;
        return 0.05f;
    }
    private int GetRandomEnemyIndex(LayerConfig config)
    {
        if (config.enemies == null || config.enemies.Count == 0) return 0;
        return Random.Range(0, config.enemies.Count);
    }

    private int GetRandomEnemyIndexFromIndex(List<EnemyData> enemies)
    {
        if (enemies == null || enemies.Count == 0) return 0;
        return Random.Range(0, enemies.Count);
    }
}
