using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeTypeWeight
{
    public NodeType nodeType;
    public int weight;
}

[CreateAssetMenu(fileName = "LayerConfig", menuName = "Stage/LayerConfig")]
public class LayerConfig : ScriptableObject
{
    public List<NodeTypeWeight> nodeTypeWeights;
    public List<EnemyData> enemies;
    public List<NodeTypeWeight> randomNodeWeight;
}
