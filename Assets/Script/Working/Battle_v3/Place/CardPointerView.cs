using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum CardPointerEventKind { Enter, Exit, Down, Up, BeginDrag, Drag, EndDrag }

// 카드 1장의 포인터 입력(Hover/누름/드래그)을 위로 보고만 하는 입력 중계 View - 판정/연출 없음.
[RequireComponent(typeof(Collider2D))]
public class CardPointerView : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPoolCallbackReceiver
{
    [SerializeField] private HandCardView card; // 같은 오브젝트의 카드 View - 미할당 시 Awake에서 자동 확보

    public HandCardView Card => card;

    //소유 View가 카드를 받을 때 구독, 넘길 때 해제
    public event Action<CardPointerView, CardPointerEventKind, PointerEventData> OnPointerEvent;

    private void Awake()
    {
        if (card == null) TryGetComponent(out card);
    }

    public void OnPointerEnter(PointerEventData eventData) => Raise(CardPointerEventKind.Enter, eventData);
    public void OnPointerExit(PointerEventData eventData) => Raise(CardPointerEventKind.Exit, eventData);
    public void OnPointerDown(PointerEventData eventData) => Raise(CardPointerEventKind.Down, eventData);
    public void OnPointerUp(PointerEventData eventData) => Raise(CardPointerEventKind.Up, eventData);
    public void OnBeginDrag(PointerEventData eventData) => Raise(CardPointerEventKind.BeginDrag, eventData);
    public void OnDrag(PointerEventData eventData) => Raise(CardPointerEventKind.Drag, eventData);
    public void OnEndDrag(PointerEventData eventData) => Raise(CardPointerEventKind.EndDrag, eventData);

    private void Raise(CardPointerEventKind kind, PointerEventData eventData)
    {
        OnPointerEvent?.Invoke(this, kind, eventData);
    }

    public void OnRent() { }

    public void OnReturn()
    {
        OnPointerEvent = null; // 풀 재사용 시 이전 소유자 구독이 남지 않게(소유 View도 해제하지만 이중 방어)
    }
}