using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    [SerializeField] private Camera backgroundCamera;
    [SerializeField] private Vector2 originalSize = new Vector2(19.2f, 10.8f);
    private int _lastScreenWidth;
    private int _lastScreenHeight;

    void Awake()
    {
        FitToCamera();
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
        {
            FitToCamera();
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
        }
    }
#endif

    public void FitToCamera()
    {
        float cameraHeight = backgroundCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * backgroundCamera.aspect;

        float scaleX = cameraWidth / originalSize.x;
        float scaleY = cameraHeight / originalSize.y;
        float scale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(scale, scale, 1f);

        float offsetY = -(originalSize.y / 2f) * scale;
        transform.position = new Vector3(
            backgroundCamera.transform.position.x,
            backgroundCamera.transform.position.y + offsetY,
            0f
        );
    }
}