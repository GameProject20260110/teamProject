using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundCoverFitter : MonoBehaviour
{
    public static BackgroundCoverFitter Instance { get; private set; }

    [SerializeField] private Camera targetCamera;

    private SpriteRenderer _spriteRenderer;

    public Vector3 CoverScale { get; private set; } = Vector3.one;

    private void Awake()
    {
        Instance = this;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (targetCamera == null) targetCamera = Camera.main;
    }

    private void Start()
    {
        Fit();
    }

    public async UniTaskVoid FitDelayed(int frames = 2)
    {
        await UniTask.DelayFrame(frames, cancellationToken: this.GetCancellationTokenOnDestroy());
        Fit();
    }

    public void Fit()
    {
        if (targetCamera == null || _spriteRenderer == null || _spriteRenderer.sprite == null) return;

        // 카메라 크기
        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;

        // 원본 이미지
        Vector2 spriteNativeSize = _spriteRenderer.sprite.bounds.size;

        // 더 큰쪽으로
        float scaleX = cameraWidth / spriteNativeSize.x;
        float scaleY = cameraHeight / spriteNativeSize.y;
        float scale = Mathf.Max(scaleX, scaleY);

        CoverScale = new Vector3(scale, scale, 1f);
        transform.localScale = CoverScale;
    }
}