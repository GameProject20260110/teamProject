using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct IciclePieceConfig
{
    public float scale;
    public float localX;
}

[System.Serializable]
public class IcicleStageDefinition
{
    public string label = "단계";
    public List<IciclePieceConfig> pieces = new List<IciclePieceConfig>();
}

[CreateAssetMenu(fileName = "IcicleStageConfig", menuName = "Board Decorations/Icicle Stage Config")]
public class IcicleStageConfig : ScriptableObject
{
    [SerializeField]
    private List<IcicleStageDefinition> stages = new List<IcicleStageDefinition>
    {
        new IcicleStageDefinition
        {
            label = "1단계",
            pieces = new List<IciclePieceConfig> { new IciclePieceConfig { scale = 0.05f, localX = 0f } },
        },
        new IcicleStageDefinition
        {
            label = "2단계",
            pieces = new List<IciclePieceConfig> { new IciclePieceConfig { scale = 0.1f, localX = 0f } },
        },
        new IcicleStageDefinition
        {
            label = "3단계",
            pieces = new List<IciclePieceConfig>
            {
                new IciclePieceConfig { scale = 0.1f, localX = 0f },
                new IciclePieceConfig { scale = 0.05f, localX = 0.5f },
            },
        },
    };

    public int StageCount => stages.Count;

    public IReadOnlyList<IciclePieceConfig> GetStage(int index) => stages[index].pieces;
}
