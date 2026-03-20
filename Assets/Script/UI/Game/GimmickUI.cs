using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GimmickUI : MonoBehaviour
{
    public Transform gimmickFrame;
    public GameObject gimmickIconPrefab;
    private List<GameObject> _gimmickIcon = new List<GameObject>();

    public void RefreshIcons(List<GimmickSo> gimmicks)
    {
        ClearIcons();
        foreach (var gimmick in gimmicks)
        {
            if (gimmick.gimmickIcon == null) continue;
            GameObject iconObj = Instantiate(gimmickIconPrefab, gimmickFrame);
            iconObj.GetComponent<Image>().sprite = gimmick.gimmickIcon;

            GimmickIconHover hover = iconObj.AddComponent<GimmickIconHover>();
            hover.gimmick = gimmick;

            _gimmickIcon.Add(iconObj);
        }
    }
    public void ClearIcons()
    {
        foreach(var icon in _gimmickIcon)
        {
            if(icon != null)
            {
                Destroy(icon);
            }
        }
        _gimmickIcon.Clear();
    }
}
