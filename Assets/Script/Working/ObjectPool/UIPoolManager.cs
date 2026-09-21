using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class UIPoolManager : MonoBehaviour
{
    public static UIPoolManager instance { get; private set; }

    [Header("풀 기본 설정")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 100;

    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools = new();

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    public GameObject Get(GameObject prefab, Transform canvasParent, Vector2 anchoredPosition)
    {
        ObjectPool<GameObject> pool = GetOrCreatePool(prefab, canvasParent);
        GameObject obj = pool.Get();

        if (obj.transform is RectTransform rt)
        {
            rt.SetParent(canvasParent, false);
            rt.anchoredPosition = anchoredPosition;
        }
        else
        {
            Debug.LogWarning($"{prefab.name}에 RectTransform이 없습니다. UI 프리팹인지 확인하세요.");
        }

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

    private ObjectPool<GameObject> GetOrCreatePool(GameObject prefab, Transform canvasParent)
    {
        if (pools.TryGetValue(prefab, out var existing))
            return existing;

        var newPool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab, canvasParent);
                var tag = obj.AddComponent<PoolTag>();
                tag.SetSourcePrefab(prefab);
                return obj;
            },
            actionOnGet: obj =>
            {
                obj.SetActive(true);
                foreach (var receiver in obj.GetComponents<IPoolCallbackReceiver>())
                    receiver.OnRent();
            },
            actionOnRelease: obj =>
            {
                foreach (var receiver in obj.GetComponents<IPoolCallbackReceiver>())
                    receiver.OnReturn();
                obj.SetActive(false);
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