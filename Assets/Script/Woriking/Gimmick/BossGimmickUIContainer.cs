using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BossGimmickUIContainer : MonoBehaviour
{
    public static BossGimmickUIContainer instance;

    [SerializeField] private GimmickIconUI gimmickIconPrefab;
    [SerializeField] private Transform container;

    private Dictionary<GimmickSo, GimmickIconUI> _icons = new();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // 보스 입장 시 기믹 수만큼 생성 (비활성 상태)
    public void Setup(List<GimmickSo> gimmicks)
    {
        Clear();

        foreach (var gimmick in gimmicks)
        {
            var icon = Instantiate(gimmickIconPrefab, container);
            icon.Setup(gimmick);
            icon.gameObject.SetActive(false); // 처음엔 꺼두기
            _icons[gimmick] = icon;
        }
    }

    // 턴 시작 시 발동 기믹 등장 연출
    public async UniTask ActivateAsync(GimmickSo gimmick)
    {
        Debug.Log(123);
        if (_icons.TryGetValue(gimmick, out var icon))
            await icon.ShowAsync();
    }

    // 턴 끝나면 숨기기
    public void Deactivate(GimmickSo gimmick)
    {
        if (_icons.TryGetValue(gimmick, out var icon))
            icon.Hide();
    }

    // 전투 종료 시 전부 제거
    public void Clear()
    {
        foreach (var icon in _icons.Values)
            Destroy(icon.gameObject);
        _icons.Clear();
    }
}