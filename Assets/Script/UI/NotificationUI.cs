using TMPro;
using UnityEngine;
using DG.Tweening;
using NUnit.Framework.Internal.Filters;
using Cysharp.Threading.Tasks.Triggers;
public class NotificationUI : MonoBehaviour
{
    public static NotificationUI instance;
    public TextMeshProUGUI notificationText;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
       gameObject.SetActive(false);
    }

    public void Show(string message, float duration = 1.5f)
    {
        notificationText.text = message;
        notificationText.alpha = 1.0f;
        gameObject.SetActive(true);

        CanvasGroup cg = GetComponent<CanvasGroup>();
        cg.alpha = 1.0f;
        cg.DOFade(0, 0.5f).SetDelay(duration).OnComplete(() => gameObject.SetActive(false));
    }
}
