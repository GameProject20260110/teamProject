using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "StageMapPool", menuName = "MapSystem/Stage Map Pool")]
public class StageMapPool : ScriptableObject
{
    [Tooltip("이 스테이지에서 랜덤으로 뽑힐 맵 후보들")]
    [SerializeField] private List<MapTemplate> templates = new();

    private MapTemplate lastPicked;

    public MapTemplate PickRandom()
    {
        if (templates == null || templates.Count == 0)
        {
            Debug.LogError($"{name}에 등록된 MapTemplate이 없습니다.");
            return null;
        }

        var candidates = templates.Count > 1 && lastPicked != null
            ? templates.Where(t => t != lastPicked).ToList()
            : templates;

        var picked = candidates[Random.Range(0, candidates.Count)];
        lastPicked = picked;
        return picked;
    }

    public void ResetHistory()
    {
        lastPicked = null;
    }
}