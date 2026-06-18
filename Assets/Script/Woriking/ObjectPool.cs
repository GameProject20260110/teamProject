using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    [Header("초기 등록 프리팹")]
    [SerializeField] private GameObject[] prefabs;

    private Dictionary<GameObject, Queue<GameObject>> pools = new();
    private Dictionary<GameObject, GameObject> originMap = new();

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        // 초기 풀 생성
        foreach (var prefab in prefabs)
            pools[prefab] = new Queue<GameObject>();
    }

    public GameObject Get(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab)) return null;

        GameObject obj = pools[prefab].Count > 0
            ? pools[prefab].Dequeue()
            : Instantiate(prefab, transform);

        originMap[obj] = prefab;
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        if (!originMap.TryGetValue(obj, out var prefab))
        {
            Debug.LogWarning($"{obj.name}은 이 풀에서 만든 오브젝트가 아닙니다.");
            Destroy(obj);
            return;
        }
        obj.SetActive(false);
        pools[prefab].Enqueue(obj);
    }
}
