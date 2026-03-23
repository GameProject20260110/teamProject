using TMPro;
using UnityEngine;
using DG.Tweening;
using NUnit.Framework.Internal.Filters;
using Cysharp.Threading.Tasks.Triggers;
public class NotificationUI : MonoBehaviour
{
    public TextMeshProUGUI notificationText;
    private CanvasGroup _cg;
    private Tween _currentTween;

    public void Show(string message, float duration = 1.5f)
    {
        _currentTween?.Kill();
        gameObject.SetActive(true);
        _cg = GetComponent<CanvasGroup>();
        notificationText.text = message;
        notificationText.alpha = 1.0f;
        _cg.blocksRaycasts = true;

        CanvasGroup cg = GetComponent<CanvasGroup>();
        cg.alpha = 1.0f;
        cg.DOFade(0, 0.5f).SetDelay(duration).OnComplete(() => gameObject.SetActive(false));
    }
}
