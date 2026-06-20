using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.EventSystems;

public class GimmickIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;

    private GimmickSo _gimmick;

    public void Setup(GimmickSo gimmick)
    {
        iconImage.sprite = gimmick.icon;
        _gimmick = gimmick;
    }

    // 기믹 발동 예고 시 등장 연출
    public async UniTask ShowAsync()
    {
        gameObject.SetActive(true);
        await PlayAppearEffect();
    }

    // 턴 끝나면 숨기기
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

        // 초기 상태
        rect.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        // 스케일 펀치 + 페이드 동시에
        var seq = DOTween.Sequence()
        .Append(rect.DOScale(original * 1.1f, 0.2f).SetEase(Ease.OutBack))
        .Join(canvasGroup.DOFade(1f, 0.2f))
        .Append(rect.DOScale(original, 0.1f));

        await seq.AsyncWaitForCompletion();
    }
}
