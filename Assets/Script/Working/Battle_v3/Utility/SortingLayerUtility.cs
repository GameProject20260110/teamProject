using UnityEngine;

public static class SortingLayerUtility
{
    private const string DefaultLayerName = "Default";

    public static int ResolveId(string layerName, string context)
    {
        if(!string.IsNullOrEmpty(layerName))
        {
            int id = SortingLayer.NameToID(layerName);
            if (SortingLayer.IsValid(id) && SortingLayer.IDToName(id) == layerName) return id;
        }

        Debug.LogWarning($"{context} SortingLayer '{layerName}'이(가) 없습니다. Tags and Layers에 추가해주세요");
        return SortingLayer.NameToID(DefaultLayerName);
    }
}
