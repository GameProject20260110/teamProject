using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GimmickUI : MonoBehaviour
{
    public Transform gimmickFrame1;
    public Transform gimmickFrame2;
    public GameObject gimmickIconPrefab;
    private List<GameObject> _gimmickIcon = new List<GameObject>();

    public void RefreshIcons(List<GimmickSo> gimmicks)
    {
        ClearIcons();
        
        for(int i = 0; i < Mathf.Min(gimmicks.Count, 2); i++)
        {
            if (gimmicks[i].gimmickIcon == null) continue;

            Transform frame = i == 0 ? gimmickFrame1 : gimmickFrame2;
            GameObject iconObj = Instantiate(gimmickIconPrefab, frame);
            iconObj.GetComponent<Image>().sprite = gimmicks[i].gimmickIcon;
            GimmickIconHover hover= iconObj.AddComponent<GimmickIconHover>();
            hover.gimmick = gimmicks[i];
            _gimmickIcon.Add(iconObj);
        }

    }
    public void ClearIcons()
    {
        foreach(var icon in _gimmickIcon)
        {
            if(icon != null)
            {
                DestroyImmediate(icon);
            }
        }
        _gimmickIcon.Clear();
    }
}
