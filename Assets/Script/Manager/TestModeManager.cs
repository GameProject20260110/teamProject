using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class TestModeManager : MonoBehaviour
{
    public static TestModeManager instance;
    public bool isTestModeActive = false;

    [Header("테스트 모드 설정")]
    public int testGold = 9999;
    public int testHeart = 3;
    public DiceData[] testAbilities;
    public int[] testValues = new int[6];
    public bool[] testDiceSlot = new bool[6];
    public List<ItemSo> testItem = new List<ItemSo>();
    public bool noGimmick = false;
    public List<GimmickSo> testGimmick = new List<GimmickSo>();
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void ApplyTestStats()
    {
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.gold = testGold;
            PlayerManager.instance.heart = testHeart;
        }
    }
}   
    
