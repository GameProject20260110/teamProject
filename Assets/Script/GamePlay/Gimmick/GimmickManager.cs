using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;

public class GimmickManager : MonoBehaviour
{
    public static GimmickManager instance;

    public List<GimmickSo> allGimmicks = new List<GimmickSo>();
    public List<GimmickSo> pendingGimmicks => PlayerManager.instance.pendingGimmicks; // 상점 패널 형식으로 바꿀 시에 프로퍼티 형식이 아닌 new List<GimmickSo>()로 바꿔야 함
    public List<GimmickSo> currentActiveGimmick = new List<GimmickSo>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void PreparePendingGimmick(int round)
    {
        pendingGimmicks.Clear();
        if(round == 11)
        {
            GimmickSo first = DrawGimmick(15);
            pendingGimmicks.Add(first);

            GimmickSo second;
            int safety = 0;
            do
            {
                second = DrawGimmick(15);
                safety++;
            } while (first == second && safety < 50);
            pendingGimmicks.Add(second);
        }
        else
        {
            int targetRound = round + 4;
            pendingGimmicks.Add(DrawGimmick(targetRound));
        }

        foreach(var gimmick in pendingGimmicks)
        {
            Debug.Log($"[기믹 예정] {gimmick.name} 레벨 : {gimmick.level} 타입 : {gimmick.gimmickType}");
        }
    }
    public void ApplyPendingGimmick(int round)
    {
        foreach(var gimmick in pendingGimmicks)
        {
            currentActiveGimmick.Add(gimmick);
            Debug.Log($"{gimmick.level}티어 {gimmick.gimmickName} 발동");
            gimmick.ExecuteGimmick();
        }
        //pendingGimmicks.Clear();
        UiController.instance.RefreshGimmickIcons(currentActiveGimmick);
    }

    private GimmickSo DrawGimmick(int round)
    {
        int rand = Random.Range(1, 101);
        int targetLevel = 1;

        if(round == 5)
        {
            if (rand <= 55) targetLevel = 1;
            else if (rand <= 85) targetLevel = 2;
            else targetLevel = 3;
        }
        else if(round == 10)
        {
            if (rand <= 30) targetLevel = 1;
            else if (rand <= 70) targetLevel = 2;
            else targetLevel = 3;
        }
        else if(round == 15)
        {
            if (rand <= 15) targetLevel = 1;
            else if (rand <= 55) targetLevel = 2;
            else targetLevel = 3;
        }

        List<GimmickSo> availableGimmick = allGimmicks.Where(g => g.level == targetLevel).ToList();

        if(round == 15)
        {
            availableGimmick = availableGimmick.Where(g => g.category != GimmickCategory.AfterEffect).ToList();
        }

        if(availableGimmick.Count == 0) return allGimmicks[Random.Range(0, allGimmicks.Count)];

        return availableGimmick[Random.Range(0, availableGimmick.Count)];
    }

    public GimmickType GetPendingMainGimmickType()
    {
        if (pendingGimmicks == null || pendingGimmicks.Count == 0) return GimmickType.None;
        return pendingGimmicks.OrderByDescending(g => g.level).First().gimmickType;

    }

    public bool IsGimmickActive(GimmickType type)
    {
        return currentActiveGimmick.Any(g => g.gimmickType == type);
    }

    public void ClearGimmick()
    {
        currentActiveGimmick.Clear();
        UiController.instance.ClearGimmickIcons();
    }
}
