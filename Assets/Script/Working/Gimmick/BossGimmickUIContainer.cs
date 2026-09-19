using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BossGimmickUIContainer : MonoBehaviour
{
    private static BossGimmickUIContainer _instance;
    public static BossGimmickUIContainer instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<BossGimmickUIContainer>(FindObjectsInactive.Include);
            return _instance;
        }
    }
        
    [SerializeField] private GimmickIconUI gimmickIconPrefab;
    [SerializeField] private Transform container;

    private Dictionary<GimmickSo, GimmickIconUI> _icons = new();

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    public void Setup(List<GimmickSo> gimmicks)
    {
        Clear();

        foreach (var gimmick in gimmicks)
        {
            var icon = Instantiate(gimmickIconPrefab, container);
            icon.Setup(gimmick);
            icon.gameObject.SetActive(false);
            _icons[gimmick] = icon;
        }
    }

    public async UniTask ActivateAsync(GimmickSo gimmick)
    {
        Debug.Log(123);
        if (_icons.TryGetValue(gimmick, out var icon))
            await icon.ShowAsync();
    }

    public void Deactivate(GimmickSo gimmick)
    {
        if (_icons.TryGetValue(gimmick, out var icon))
            icon.Hide();
    }


    public void Clear()
    {
        foreach (var icon in _icons.Values)
            Destroy(icon.gameObject);
        _icons.Clear();
    }
}