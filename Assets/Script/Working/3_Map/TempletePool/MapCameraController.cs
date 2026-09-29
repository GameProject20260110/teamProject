using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MapCameraController : MonoBehaviour
{
    [SerializeField] private RectTransform mapCanvasRect;
    [SerializeField] private Ease defaultEase = Ease.InOutCubic;

    private Camera cam;
    private float baseSize;
    private float fixedZ;
    private Sequence current;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        baseSize = cam.orthographicSize;
        fixedZ = transform.position.z;
    }

    public UniTask ZoomToAsync(Vector3 worldTarget, float zoom, float duration, Ease? ease = null)
    {
        zoom = Mathf.Max(1f, zoom);
        float targetSize = baseSize / zoom;
        Vector3 targetPos = ClampToCanvas(worldTarget, targetSize);
        return PlayAsync(targetPos, targetSize, duration, ease);
    }

    public UniTask ResetAsync(float duration, Ease? ease = null)
    {
        Vector3 center = ClampToCanvas(GetCanvasWorldRect().center, baseSize);
        return PlayAsync(center, baseSize, duration, ease);
    }

    private async UniTask PlayAsync(Vector3 pos, float size, float duration, Ease? ease)
    {
        current?.Kill();
        var e = ease ?? defaultEase;

        current = DOTween.Sequence();
        _ = current.Join(transform.DOMove(pos, duration).SetEase(e));
        _ = current.Join(cam.DOOrthoSize(size, duration).SetEase(e));

        await current.AsyncWaitForCompletion();
    }

    private Rect GetCanvasWorldRect()
    {
        var corners = new Vector3[4];
        mapCanvasRect.GetWorldCorners(corners);
        return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
    }

    private Vector3 ClampToCanvas(Vector3 target, float size)
    {
        Rect r = GetCanvasWorldRect();
        float halfH = size;
        float halfW = size * cam.aspect;

        float x = r.width <= halfW * 2f ? r.center.x : Mathf.Clamp(target.x, r.xMin + halfW, r.xMax - halfW);
        float y = r.height <= halfH * 2f ? r.center.y : Mathf.Clamp(target.y, r.yMin + halfH, r.yMax - halfH);

        return new Vector3(x, y, fixedZ);
    }
}
