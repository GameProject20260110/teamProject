using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class TestModeManager : MonoBehaviour
{
    public static TestModeManager instance;
    public bool isTestModeActive = false;

    [Header("테스트 모드 설정")]
    public DiceData[] testAbilities;
    public int[] testValues = new int[6];

    public bool[] testDiceSlot = new bool[6];

    public List<ItemSo> testItem = new List<ItemSo>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
}   
    
