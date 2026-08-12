using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using VContainer;

public class MapIntroController : MonoBehaviour
{
    [Header("오버레이")]
    public Image darkOverlay;
    public CanvasGroup NodeInfoImage;

    [Header("지도 두루마리")]
    public Transform mapMask;
    public SpriteMask mapMaskComponent;
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
    private SceneController _sceneController;

    [Inject]
    public void Construct(SceneController sceneController)
    {
        _sceneController = sceneController;
    }

    public void PlayIntro()
    {
        _sceneController.isFirstEntry = false;
        cts = new CancellationTokenSource();
        PlayIntroAsync(cts.Token).Forget();
    }

    public void SkipIntro()
    {
        darkOverlay.color = new Color(0, 0, 0, 0);
        mapMask.localScale = new Vector3(mapOriginalScaleX, mapOriginalScaleY, 1f);
        mapRollRenderer.color = new Color(1f, 1f, 1f, 1f);

        SnapRollToMaskBottom();
        MapCameraController.Instance?.RecalculateBounds();
    }

    private async UniTask PlayIntroAsync(CancellationToken ct)
    {
        // 초기 상태
        darkOverlay.color = new Color(0, 0, 0, 1f);
        NodeInfoImage.alpha = 0f;

        float topY = 9.5f;
        mapMask.localPosition = new Vector3(mapMask.position.x, topY, 0f);
        mapMask.localScale = new Vector3(mapOriginalScaleX, 0f, 1f);
        mapRoll.localPosition = new Vector3(mapRoll.position.x, topY, 0f);
        mapRollRenderer.color = new Color(1f, 1f, 1f, 0f);

        await darkOverlay.DOFade(220f / 255f, 0.8f).SetEase(Ease.OutQuad);
        await mapRollRenderer.DOColor(new Color(1f, 1f, 1f, 1f), 1f).SetEase(Ease.InOutQuad);

        float t = 0f;
        await DOTween.To(() => t, x => t = x, 1f, 1.7f)
            .SetEase(Ease.InOutQuad)
            .OnUpdate(() =>
            {
                float scaleY = Mathf.Lerp(0f, mapOriginalScaleY, t);
                mapMask.localScale = new Vector3(mapOriginalScaleX, scaleY, 1f);
                SnapRollToMaskBottom();
            })
            .AsyncWaitForCompletion();

        MapCameraController.Instance?.RecalculateBounds();

        // 5단계: 제 1막 띠 등장
        await UniTask.Delay(300, cancellationToken: ct);
        await NodeInfoImage.DOFade(1f, 0.3f).SetEase(Ease.OutQuad);
        bool bannerDone = false;
        roundIntroController.Play("<size=40><color=#FFD700>제 1막</color></size>\n<size=60>시련의 시작</size>", () => bannerDone = true);
        MapManager.Instance.StartIntro();
        await UniTask.WaitUntil(() => bannerDone, cancellationToken: ct);
    }

    private void SnapRollToMaskBottom()
    {
        if (mapMaskComponent == null || mapMaskComponent.sprite == null)
        {
            Debug.LogWarning("MapIntroController");
            return;
        }

        Bounds localBounds = mapMaskComponent.sprite.bounds;
        Vector3 bottomLocalPoint = new Vector3(localBounds.center.x, localBounds.min.y, 0f);
        Vector3 bottomWorldPoint = mapMaskComponent.transform.TransformPoint(bottomLocalPoint);

        Vector3 worldPoint = new Vector3(mapRoll.position.x, bottomWorldPoint.y, mapRoll.position.z);
        Transform parent = mapRoll.parent;
        Vector3 localPoint = parent != null ? parent.InverseTransformPoint(worldPoint) : worldPoint;

        mapRoll.localPosition = new Vector3(mapRoll.localPosition.x, localPoint.y, 0f);
    }
}