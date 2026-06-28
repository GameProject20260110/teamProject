using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

public class LetterBoxUI : MonoBehaviour
{
    public static LetterBoxUI instance;

    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private float barHeight = 120f;
    [SerializeField] private float duration = 0.5f;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        //topBar.gameObject.SetActive(false);
        //bottomBar.gameObject.SetActive(false);
    }

    public async UniTask Show(CancellationToken ct)
    {
        if(topBar != null && bottomBar != null)
        {
            topBar.gameObject.SetActive(true);
            bottomBar.gameObject.SetActive(true);
        }

        await UniTask.WhenAll(
            topBar.DOAnchorPosY(0, duration)
                .SetEase(Ease.OutQuad)
                .ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: ct),
            bottomBar.DOAnchorPosY(0, duration)
                .SetEase(Ease.OutQuad)
                .ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: ct)
        );  
    }

    public async UniTask Hide(CancellationToken ct)
    {
        await UniTask.WhenAll(
            topBar.DOAnchorPosY(barHeight, duration)
                .SetEase(Ease.InQuad)
                .ToUniTask(TweenCancelBehaviour.Kill,cancellationToken: ct),
            bottomBar.DOAnchorPosY(-barHeight, duration)
                .SetEase(Ease.InQuad)
                .ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: ct)
        );

        if (topBar != null && bottomBar != null)
        {
            topBar.gameObject.SetActive(false);
            bottomBar.gameObject.SetActive(false);
        }
    }
}
