using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;

    public enum PoolType
    {
        Fireball = 0,
        IceShard = 1,
        Enemy = 2,
    }

    [SerializeField] private GameObject[] prefabs;

    private Queue<GameObject>[] pools;
    private Dictionary<GameObject, int> prefabIndexMap;

    void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);

        pools = new Queue<GameObject>[prefabs.Length];
        prefabIndexMap = new Dictionary<GameObject, int>();

        for (int i = 0; i < prefabs.Length; i++)
        {
            pools[i] = new Queue<GameObject>();
        }
    }

    public GameObject Get(int index)
    {
        GameObject obj;

        if (pools[index].Count > 0)
        {
            obj = pools[index].Dequeue();
        }
        else
        {
            obj = Instantiate(prefabs[index], transform);
            prefabIndexMap[obj] = index;
        }

        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        if (!prefabIndexMap.TryGetValue(obj, out int index))
        {
            Debug.LogWarning($"{obj.name}은 이 풀에서 만든 오브젝트가 아닙니다.");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        pools[index].Enqueue(obj);
    }
}