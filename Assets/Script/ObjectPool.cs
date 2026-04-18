using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public enum PoolType
    {
        Electric = 0,
        Fireball = 1,
    }

    public static ObjectPool instance;

    [SerializeField] private GameObject[] prefabs;

    private Queue<GameObject>[] pools;
    private Dictionary<GameObject, int> prefabIndexMap; // 반환 시 인덱스 찾기용

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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
            prefabIndexMap[obj] = index; // 어느 풀 소속인지 기록
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
