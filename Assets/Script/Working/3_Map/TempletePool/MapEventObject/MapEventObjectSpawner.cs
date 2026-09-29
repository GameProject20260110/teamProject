using UnityEngine;

public class MapEventObjectSpawner
{
    private MapEventObject current;

    public MapEventObject Spawn(RectTransform parentNode, MapEventObject prefab)
    {
        Despawn();

        if (prefab == null) return null;

        current = Object.Instantiate(prefab, parentNode);
        ((RectTransform)current.transform).anchoredPosition = Vector3.zero;

        return current;
    }

    public void Despawn()
    {
        if (current != null)
        {
            DG.Tweening.DOTween.Kill(current.transform);
            Object.Destroy(current.gameObject);
        }
        current = null;
    }
}
