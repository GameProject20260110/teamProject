using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ChestEventObject : MapEventObject
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private float dropHeight = 600f;
    [SerializeField] private Vector2 landingOffset = new Vector2(70f, 0f);

    private Sequence sequence;

    public override async UniTask PlayAsync()
    {
        var rect = (RectTransform)transform;
        image.sprite = closedSprite;
        rect.anchoredPosition = landingOffset + new Vector2(0f, dropHeight);
        
        sequence = DOTween.Sequence();

        _ = sequence.Append(rect.DOAnchorPos(landingOffset, 0.6f).SetEase(Ease.OutBounce));
        _ = sequence.AppendInterval(0.3f);

        _ = sequence.AppendCallback(() => image.sprite = openSprite);
        _ = sequence.Append(rect.DOPunchScale(Vector3.one * 0.2f, 0.3f));

        _ = sequence.AppendInterval(0.8f);

        _ = sequence.Append(image.DOFade(0f, 0.3f));

        await sequence.AsyncWaitForCompletion();
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }
}
