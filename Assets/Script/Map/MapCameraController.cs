using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapCameraController : MonoBehaviour
{
    public static MapCameraController instance;

    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Camera mapCamera;
    [SerializeField] private float dragSpeed = 1f;
    [SerializeField] private float cameraMoveDuration = 1.0f;
    [SerializeField] private float dragThreshold = 5f;

    private Vector2 _dragStartPos;
    private float _minY;
    private float _maxY;
    public bool IsDragging { get; private set; }


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        if (background != null)
        {
            float camHalfHeight = mapCamera.orthographicSize;
            _minY = background.bounds.min.y + camHalfHeight;
            _maxY = background.bounds.max.y - camHalfHeight;
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
        float clampedY = Mathf.Clamp(targetY, _minY, _maxY);
        mapCamera.transform.DOMoveY(clampedY, cameraMoveDuration).SetEase(Ease.InOutQuad);
    }

    public void MoveToLayerImmediate(float targetY)
    {
        float clampedY = Mathf.Clamp(targetY, _minY, _maxY);
        mapCamera.transform.position = new Vector3(mapCamera.transform.position.x, clampedY, mapCamera.transform.position.z);
    }
}
