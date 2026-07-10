using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BurstEffect : MonoBehaviour, IPoolCallbackReceiver
{
    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    public void OnRent()
    {
        DOTween.Kill(transform);
        DOTween.Kill(img);

        var c = img.color;
        c.a = 0.8f; 
        img.color = c;

        transform.localScale = Vector3.one;
    }

    public void OnReturn()
    {
        DOTween.Kill(transform);
        DOTween.Kill(img);
    }
}