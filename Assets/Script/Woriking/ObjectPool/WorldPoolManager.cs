using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class WorldPoolManager : MonoBehaviour
{
    public static WorldPoolManager instance { get; private set; }

    [Header("풀 기본 설정")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 100;

    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools = new();
    private readonly Dictionary<GameObject, Vector3> _prefabScales = new();


    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        ObjectPool<GameObject> pool = GetOrCreatePool(prefab);
        GameObject obj = pool.Get();

        obj.transform.SetParent(parent, false);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.transform.localScale = _prefabScales[prefab];

        return obj;
    }

    public void Return(GameObject obj)
    {
        var tag = obj.GetComponent<PoolTag>();
        if (tag != null && pools.TryGetValue(tag.SourcePrefab, out var pool))
        {
            pool.Release(obj);
        }
        else
        {
            Debug.LogWarning($"{obj.name}은 이 풀에서 만든 오브젝트가 아닙니다.");
            Destroy(obj);
        }
    }

    private ObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out var existing))
            return existing;

        if (!_prefabScales.ContainsKey(prefab))
            _prefabScales[prefab] = prefab.transform.localScale;

        var newPool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab, transform);
                var tag = obj.AddComponent<PoolTag>();
                tag.SetSourcePrefab(prefab);
                return obj;
            },
            actionOnGet: obj =>
            {
                obj.SetActive(true);
                foreach (var receiver in obj.GetComponents<IPoolCallbackReceiver>())
                    receiver.OnRent();
                obj.transform.DOKill();
                obj.transform.SetParent(transform, false);
            },
            actionOnRelease: obj =>
            {
                foreach (var receiver in obj.GetComponents<IPoolCallbackReceiver>())
                    receiver.OnReturn();
                obj.SetActive(false);
                obj.transform.SetParent(transform, false);
            },
            actionOnDestroy: obj => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        pools[prefab] = newPool;
        return newPool;
    }
}