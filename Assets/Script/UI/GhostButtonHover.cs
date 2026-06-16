using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GhostButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image borderImage;
    [SerializeField] private float fadeDuration = 0.1f;

    private void Awake()
    {
        SetAlpha(0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        borderImage.DOKill();

        borderImage.DOFade(1f, fadeDuration).SetLink(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        borderImage.DOKill();

        borderImage.DOFade(0f, fadeDuration).SetLink(gameObject);
    }

    private void SetAlpha(float alpha)
    {
        if (borderImage == null) return;

        Color c = borderImage.color;
        c.a = alpha;
        borderImage.color = c;
    }
}
