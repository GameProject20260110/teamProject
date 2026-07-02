using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

public class MapIntroController : MonoBehaviour
{
    public static MapIntroController instancce;

    [Header("오버레이")]
    public Image darkOverlay;
    public CanvasGroup NodeInfoImage;

    [Header("텍스트")]
    public TypeWriter typeWriter;
    public CanvasGroup textGroup;

    [Header("지도 두루마리")]
    public Transform mapMask;
    public Transform mapRoll;
    public SpriteRenderer mapRollRenderer;
    public float mapOriginalScaleY;
    public float mapOriginalScaleX;

    [Header("카메라")]
    public float cameraStartY;
    public float cameraEndY;

    [Header("제 1막 띠")]
    public RoundIntroController roundIntroController;

    private CancellationTokenSource cts;

    private void Awake()
    {
        if (instancce == null) instancce = this;
        else Destroy(gameObject);
    }

    public void PlayIntro()
    {
        SceneController.instance.isFirstEntry = false;
        cts = new CancellationTokenSource();
        PlayIntroAsync(cts.Token).Forget();
    }

    public void SkipIntro()
    {
        darkOverlay.color = new Color(0, 0, 0, 0);
        mapMask.localScale = new Vector3(mapOriginalScaleX, mapOriginalScaleY, 1f);
        mapRoll.localPosition = new Vector3(mapRoll.localPosition.x, -5.4f, 0f);
        mapRollRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    private async UniTask PlayIntroAsync(CancellationToken ct)
    {
        // 초기 상태
        darkOverlay.color = new Color(0, 0, 0, 1f);
        NodeInfoImage.alpha = 0f;
        textGroup.alpha = 0f;
        mapMask.localPosition = new Vector3(mapMask.position.x, 5.4f, 0f);
        mapMask.localScale = new Vector3(mapOriginalScaleX, 0f, 1f);
        mapRoll.localPosition = new Vector3(mapRoll.position.x, 5.4f, 0f);
        mapRollRenderer.color = new Color(1f, 1f, 1f, 0f);

        // 1단계: 씬 로드 직후 잠깐 대기
        await UniTask.Delay(500, cancellationToken: ct);

        string[] texts = {
        "태초의 완벽한 구조가 무너졌다.",
        "카오스가 세계를 집어삼키고 있다.",
        "주사위를 모아라.",
        "구조를 완성하라."
        };
        foreach (var text in texts)
            await TextAppear(text, ct);

        await darkOverlay.DOFade(220f / 255f, 0.8f).SetEase(Ease.OutQuad);
        await mapRollRenderer.DOColor(new Color(1f, 1f, 1f, 1f), 1f).SetEase(Ease.InOutQuad);

        DOTween.To(
            () => mapMask.localScale.y,
            y => mapMask.localScale = new Vector3(mapOriginalScaleX, y, 1f),
            mapOriginalScaleY, 1.5f
        ).SetEase(Ease.InOutQuad);

        await DOTween.To(
            () => mapRoll.localPosition.y,
            y => mapRoll.localPosition = new Vector3(mapRoll.localPosition.x, y, 0f),
            -5.4f, 1.5f
        ).SetEase(Ease.InOutQuad)
        .AsyncWaitForCompletion();

        // 5단계: 제 1막 띠 등장
        await UniTask.Delay(300, cancellationToken: ct);

        await NodeInfoImage.DOFade(1f, 0.3f).SetEase(Ease.OutQuad);

        bool bannerDone = false;
        roundIntroController.Play("<size=40><color=#FFD700>제 1막</color></size>\n<size=60>시련의 시작</size>", () => bannerDone = true);

        

        MapManager.instance.StartIntro();
        await UniTask.WaitUntil(() => bannerDone, cancellationToken: ct);
        // 끝
    }

    private async UniTask TextAppear(string text, CancellationToken ct)
    {
        textGroup.alpha = 1f;
        await typeWriter.Play(text, ct);
        await UniTask.Delay(800, cancellationToken: ct);
        await textGroup.DOFade(0f, 0.5f).AsyncWaitForCompletion();
        await UniTask.Delay(400, cancellationToken: ct);
    }

    void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}