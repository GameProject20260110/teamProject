using DG.Tweening;
using UnityEngine;

public class MagicCircleEffect : MonoBehaviour
{
    [SerializeField] private RectTransform circle1;
    [SerializeField] private RectTransform circle2;    

    public Sequence PlaySealEffect(Vector2 position)
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        transform.position = position;


        circle1.DORotate(new Vector3(0, 0, 360), 3f, RotateMode.FastBeyond360)
               .SetLoops(-1, LoopType.Restart)
               .SetEase(Ease.Linear);

        circle2.DORotate(new Vector3(0, 0, -360), 3f, RotateMode.FastBeyond360)
               .SetLoops(-1, LoopType.Restart)
               .SetEase(Ease.Linear);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
        seq.AppendInterval(1.0f);
        seq.Append(transform.DOScale(0f, 0.4f).SetEase(Ease.InBack));
        seq.OnComplete(() =>
        {
            circle1.DOKill();
            circle2.DOKill();
            gameObject.SetActive(false);
        });
            
        return seq;

    }
}
