using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class LightOrbTransition : MonoBehaviour
{
    [Header("빛 공")]
    [SerializeField] private RectTransform orbRect;         // 빛 공 Image
    [SerializeField] private Image orbImage;
    [SerializeField] private Color orbColor = new Color(0.5f, 0.8f, 1f, 1f);  // 파란빛

    [Header("이동 설정")]
    [SerializeField] private RectTransform startPoint;      // 시작점 (Round 텍스트 위치)
    [SerializeField] private RectTransform targetImage;     // 도착할 UI 이미지
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private float arcHeight = 300f;        // 포물선 높이 (클수록 더 휨)
    [SerializeField] private Ease moveEase = Ease.InOutQuad;

    [Header("빛 공 크기")]
    [SerializeField] private float orbStartScale = 1f;
    [SerializeField] private float orbEndScale = 0.2f;      // 이동하면서 점점 작아짐

    [Header("도착 효과")]
    [SerializeField] private float targetFadeInDuration = 0.3f;
    [SerializeField] private float arrivalFlashDuration = 0.15f;  // 도착 순간 번쩍임

    [Header("레퍼런스")]
    [SerializeField] private GameObject RoundContainer;

    private Sequence currentSequence;

    void Awake()
    {
        // 빛 공은 평소엔 숨김
        if (orbRect != null) orbRect.gameObject.SetActive(false);

        // 타겟 이미지는 평소엔 투명
        if (targetImage != null)
        {
            Image img = targetImage.GetComponent<Image>();
            if (img != null) SetAlpha(img, 0f);
        }
    }

    /// <summary>
    /// 외부에서 호출: startPos에서 빛 공 생성 후 targetImage로 이동
    /// </summary>
    public void Play(Vector2 startAnchoredPos, Action onComplete = null)
    {
        currentSequence?.Kill();
        orbRect.gameObject.SetActive(true);

        // 빛 공 초기화
        orbRect.anchoredPosition = startAnchoredPos;
        orbRect.localScale = Vector3.one * orbStartScale;
        SetAlpha(orbImage, 1f);
        orbImage.color = orbColor;

        currentSequence = DOTween.Sequence();

        Vector2 targetPos = (Vector2)orbRect.parent.InverseTransformPoint(targetImage.position);

        // 포물선 제어점 (시작과 끝 사이 왼쪽 위)
        Vector2 controlPoint = new Vector2(
            (startAnchoredPos.x + targetPos.x) * 0.5f - arcHeight, 0
        );

        // t: 0 → 1 로 보간하면서 2차 베지어 곡선으로 위치 계산
        float t = 0f;
        DOTween.To(
            () => t,
            value => {
                t = value;
                // 2차 베지어 곡선 공식
                // B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
                float oneMinusT = 1f - t;
                Vector2 pos = (oneMinusT * oneMinusT * startAnchoredPos)
                            + (2f * oneMinusT * t * controlPoint)
                            + (t * t * targetPos);
                orbRect.anchoredPosition = pos;
            },
            1f,           // 목표값
            moveDuration  // 시간
        ).SetEase(moveEase)
         .OnComplete(() => OnArrival(onComplete));

        currentSequence.Join(
            orbRect.DOScale(orbEndScale, moveDuration).SetEase(Ease.InQuad)
        );
        currentSequence.Join(
            orbImage.DOFade(0.3f, moveDuration).SetEase(Ease.InQuad)
        );
        currentSequence.AppendCallback(() => OnArrival(onComplete));
    }

    private void OnArrival(Action onComplete)
    {
        orbRect.gameObject.SetActive(false);

        //Image img = targetImage.GetComponent<Image>();
        if (RoundContainer != null)
        {
            //// 도착 순간 번쩍임 (알파 확 올라갔다가 정상으로)
            //img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);

            //Sequence arrivalSeq = DOTween.Sequence();

            //// 번쩍이며 등장
            //arrivalSeq.Append(img.DOFade(1.2f, arrivalFlashDuration * 0.5f).SetEase(Ease.OutQuad));
            //arrivalSeq.Append(img.DOFade(1f, arrivalFlashDuration * 0.5f).SetEase(Ease.InQuad));

            //arrivalSeq.OnComplete(() => onComplete?.Invoke());

            RoundContainer.SetActive(true);
            onComplete?.Invoke();
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    void OnDestroy()
    {
        currentSequence?.Kill();
        orbRect?.DOKill();
        orbImage?.DOKill();
    }

    // 유틸
    private void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
