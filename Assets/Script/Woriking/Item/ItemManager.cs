using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;

    public List<BattleItemSo> items = new List<BattleItemSo>();
    public List<BattleItemSo> Artifacts = new List<BattleItemSo>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

}
