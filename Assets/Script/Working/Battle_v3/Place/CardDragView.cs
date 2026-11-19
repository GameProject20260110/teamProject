using System;
using UnityEngine;


public class CardDragView : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;   
    [SerializeField] private PlacementConfig config;

    //창 포커스를 잃는 등 EndDrag가 안 올 수 있는 상황(드래그를 원래 자리로 되돌려야 함)
    public event Action OnDragAborted;

    private HandCardView _card;
    private Vector3 _grabOffset;   // 집은 지점 <-> 카드 중심 차이(집는 순간 카드가 튀지 않게)
    private float _planeZ;         // 카드가 움직이는 평면(월드 z)
    private int _dragLayerId;

    public bool IsDragging => _card != null;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        _dragLayerId = SortingLayerUtility.ResolveId(config != null ? config.dragSortingLayer : null, nameof(CardDragView));
    }

    public void BeginDrag(HandCardView cardView, Vector2 screenPos)
    {
        if (cardView == null) return; 
        _card = cardView;
        _planeZ = cardView.transform.position.z;

        Vector3 pointer = ScreenToWorld(ClampToScreen(screenPos));
        _grabOffset = cardView.transform.position - pointer;
        _grabOffset.z = 0f;

        float scale = config != null ? config.dragScale : 1.15f;
        float duration = config != null ? config.dragPoseDuration : 0.08f;
        cardView.SetSorting(_dragLayerId, config != null ? config.dragSortingOrder : 0);
        cardView.PlayDragPose(scale, duration);
    }

    // 포인터를 따라감, 화면 밖으로 나가면 화면 가장자리에 붙음(카드가 사라지지 않게)
    public void Drag(Vector2 screenPos)
    {
        if (_card == null) return;
        _card.SetWorldPosition(ScreenToWorld(ClampToScreen(screenPos)) + _grabOffset);
    }

    // 놓기 - 이후 위치는 받는 쪽(손패/패널)이 이동시킴
    public void EndDrag()
    {
        _card = null;
    }

    // 판정용 포인터 월드 좌표. 화면 밖이면 false
    public bool TryGetPointerWorld(Vector2 screenPos, out Vector3 worldPos)
    {
        worldPos = Vector3.zero;
        if (targetCamera == null) return false;
        if (screenPos.x < 0f || screenPos.y < 0f || screenPos.x > Screen.width || screenPos.y > Screen.height) return false;

        worldPos = ScreenToWorld(screenPos);
        return true;
    }

    private static Vector2 ClampToScreen(Vector2 screenPos)
    {
        return new Vector2(Mathf.Clamp(screenPos.x, 0f, Screen.width), Mathf.Clamp(screenPos.y, 0f, Screen.height));
    }

    // 화면 좌표 -> 카드 평면(_planeZ) 위 월드 좌표.
    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        if (targetCamera == null) return Vector3.zero;
        float depth = Mathf.Abs(_planeZ - targetCamera.transform.position.z);
        Vector3 world = targetCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, depth));
        world.z = _planeZ;
        return world;
    }

    // 드래그 중 창 포커스를 잃으면 버튼 뗌(EndDrag)이 안 들어올 수 있음 -> 되돌리기 요청.
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && _card != null) OnDragAborted?.Invoke();
    }
}