using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageText : MonoBehaviour, IPoolCallbackReceiver
{
    private RectTransform rt;
    private TextMeshProUGUI tmp;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        tmp = GetComponent<TextMeshProUGUI>();
    }

    public void OnRent()
    {
        DOTween.Kill(rt);
        DOTween.Kill(tmp);

        var c = tmp.color;
        c.a = 1f;
        tmp.color = c;
    }

    public void OnReturn()
    {
        DOTween.Kill(rt);
        DOTween.Kill(tmp);
    }
}
