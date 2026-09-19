using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.EventSystems;

public class GimmickIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;

    private GimmickSo _gimmick;

    public void Setup(GimmickSo gimmick)
    {
        iconImage.sprite = gimmick.icon;
        _gimmick = gimmick;
    }

    public async UniTask ShowAsync()
    {
        gameObject.SetActive(true);
        await PlayAppearEffect();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipPanel.SetActive(true);
        tooltipText.text = _gimmick.description;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);
    }

    private async UniTask PlayAppearEffect()
    {
        var rect = GetComponent<RectTransform>();
        Vector3 original = rect.localScale;

        rect.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        var seq = DOTween.Sequence()
        .Append(rect.DOScale(original * 1.1f, 0.2f).SetEase(Ease.OutBack))
        .Join(canvasGroup.DOFade(1f, 0.2f))
        .Append(rect.DOScale(original, 0.1f));

        await seq.AsyncWaitForCompletion();
    }
}
