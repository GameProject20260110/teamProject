using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GimmickCardDatabase", menuName = "Battle/Gimmick Card Database")]
public class GimmickCardDatabase : ScriptableObject
{
    [SerializeField] private List<GimmickCardData> allGimmicks = new List<GimmickCardData>();

    public IReadOnlyList<GimmickCardData> All => allGimmicks;

    private Dictionary<string, GimmickCardData> _byId;

    public GimmickCardData GetById(string gimmickId)
    {
        if (_byId == null) BuildLookup();
        return _byId.TryGetValue(gimmickId, out var data) ? data : null;
    }

    private void BuildLookup()
    {
        _byId = new Dictionary<string, GimmickCardData>();
        foreach (var g in allGimmicks)
        {
            if (g == null) continue;
            if (!_byId.TryAdd(g.GimmickId, g))
                Debug.LogWarning($"[GimmickCardDatabase] Duplicate gimmick id '{g.GimmickId}' on {g.name} — ignored.");
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var seen = new HashSet<string>();
        foreach (var g in allGimmicks)
        {
            if (g == null) continue;
            if (string.IsNullOrEmpty(g.GimmickId))
                Debug.LogWarning($"[GimmickCardDatabase] {g.name} has an empty GimmickId.", g);
            else if (!seen.Add(g.GimmickId))
                Debug.LogWarning($"[GimmickCardDatabase] Duplicate GimmickId '{g.GimmickId}' found ({g.name}).", g);
        }
    }
#endif
}
