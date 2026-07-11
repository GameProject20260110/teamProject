using UnityEngine;

public class PoolTag : MonoBehaviour
{
    public GameObject SourcePrefab { get; private set; }

    public void SetSourcePrefab(GameObject prefab) => SourcePrefab = prefab;
}
