using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapCameraController : MonoBehaviour
{
    public static MapCameraController Instance { get; private set; }

    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Camera mapCamera;
    [SerializeField] private float dragSpeed = 1f;
    [SerializeField] private float cameraMoveDuration = 1.0f;
    [SerializeField] private float dragThreshold = 5f;

    [Header("카메라 줌인 세팅")]
    [SerializeField] private float zoomSize = 1.5f;
    [SerializeField] private float zoomDuration = 0.4f;
    [SerializeField] private float zoomMoveDuration = 0.5f;

    private float _originalSize;
    private Vector3 _originalPosition;
    private Vector2 _dragStartPos;
    private float _minY;
    private float _maxY;
    public bool IsDragging { get; private set; }


    private void Awake()
    {
        Instance = this;

        if (background != null)
        {
            float camHalfHeight = mapCamera.orthographicSize;
            _minY = background.bounds.min.y + camHalfHeight;
            _maxY = background.bounds.max.y - camHalfHeight;
        }

        if (mapCamera != null)
        {
            _originalSize = mapCamera.orthographicSize;
            _originalPosition = mapCamera.transform.position;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _dragStartPos = Input.mousePosition;
            IsDragging = false;
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - _dragStartPos;
            if (delta.magnitude > dragThreshold)
            {
                IsDragging = true;

                float newY = mapCamera.transform.position.y + (-delta.y * dragSpeed);
                newY = Mathf.Clamp(newY, _minY, _maxY);

                mapCamera.transform.position = new Vector3(mapCamera.transform.position.x, newY, mapCamera.transform.position.z);

                _dragStartPos = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            IsDragging = false;
        }
    }

    public void MoveToLayer(float targetY)
    {
        mapCamera.transform.DOKill();
        float clampedY = Mathf.Clamp(targetY, _minY, _maxY);
        mapCamera.transform.DOMoveY(clampedY, cameraMoveDuration).SetEase(Ease.InOutQuad).SetLink(mapCamera.gameObject);
    }

    public void MoveToLayerImmediate(float targetY)
    {
        float clampedY = Mathf.Clamp(targetY, _minY, _maxY);
        mapCamera.transform.position = new Vector3(mapCamera.transform.position.x, clampedY, mapCamera.transform.position.z);
    }

    public async UniTask ZoomToNode(Vector3 position)
    {
        if (mapCamera == null) return;

        _originalSize = mapCamera.orthographicSize;
        _originalPosition = mapCamera.transform.position;

        Vector3 targetPos = new Vector3(position.x, position.y, mapCamera.transform.position.z);

        mapCamera.transform.DOMove(targetPos, zoomMoveDuration)
            .SetEase(Ease.InOutQuad)
            .SetLink(mapCamera.gameObject);

        await DOTween.To(
            () => mapCamera.orthographicSize,
            x => mapCamera.orthographicSize = x, zoomSize, zoomDuration)
            .SetEase(Ease.InOutQuad)
            .SetLink(mapCamera.gameObject)
            .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    public void ResetZoom()
    {
        if (mapCamera == null) return;

        mapCamera.transform.DOKill();
        mapCamera.orthographicSize = _originalSize;
        mapCamera.transform.position = _originalPosition;
    }
}
