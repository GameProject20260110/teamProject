using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;

public class GimmickManager : MonoBehaviour
{
    public static GimmickManager instance;

    public List<GimmickSo> allGimmicks = new List<GimmickSo>();

    public List<GimmickSo> currentActiveGimmick = new List<GimmickSo>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void ApplyGimmick(int round)
    {
        ClearGimmick();

        if(round == 15)
        {
            GimmickSo firstGimmick = DrawGimmick(15);
            currentActiveGimmick.Add(firstGimmick);

            GimmickSo secondGimmick;
            int safety = 0;
            do
            {
                secondGimmick = DrawGimmick(15);
                safety++;
            } while (secondGimmick == firstGimmick && safety < 50);

            currentActiveGimmick.Add(secondGimmick);
        }
        else
        {
             currentActiveGimmick.Add(DrawGimmick(round));
        }

        foreach(var gimmick in currentActiveGimmick)
        {
            Debug.Log($"{gimmick.level}티어 {gimmick.gimmickName} 발동");
            gimmick.ExecuteGimmick();
        }
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
