using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class TitleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject leftIcon;
    public GameObject rightIcon;

    public void OnPointerEnter(PointerEventData eventData)
    {
        leftIcon.SetActive(true);
        rightIcon.SetActive(true);

        leftIcon.transform.DOKill();
        rightIcon.transform.DOKill();

        leftIcon.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        rightIcon.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        leftIcon.transform.DOScale(0f, 0.15f)
            .SetEase(Ease.InBack)
            .OnComplete(() => leftIcon.SetActive(false));
        rightIcon.transform.DOScale(0f, 0.15f)
            .SetEase(Ease.InBack)
            .OnComplete(() => rightIcon.SetActive(false));
    }
}
