using UnityEngine;
using System.Collections.Generic;
using VContainer;

public class DeckGridController : MonoBehaviour
{
    [SerializeField] private RectTransform slotContainer;
    [SerializeField] private GameObject deckSlotPrefab;
    [SerializeField] private CardDetailPanelController detailPanel;

    [Header("우클릭 이동 효과음")]
    [SerializeField] private string cardMoveSfxKey;

    private readonly List<GameObject> _spawnedSlots = new List<GameObject>();
    private DeckEditSession _session;
    private CardDatabase _cardDatabase;
    private DeckPopupService _popupService;
    private AudioManager _audioManager;


    [Inject]
    public void Construct(CardDatabase cardDatabase, DeckPopupService popupService, AudioManager audioManager)
    {
        _cardDatabase = cardDatabase;
        _popupService = popupService;
        _audioManager = audioManager;
    }

    public void Bind(DeckEditSession session)
    {
        if (_session != null) _session.OnChanged -= Rebuild;
        _session = session;
        _session.OnChanged += Rebuild;
        Rebuild();
    }

    private void OnDestroy()
    {
        if (_session != null) _session.OnChanged -= Rebuild;
    }

    private void Rebuild()
    {
        if (_session == null || slotContainer == null || deckSlotPrefab == null || UIPoolManager.instance == null) return;

        IReadOnlyList<int> ids = _session.SlotCardIds;

        while (_spawnedSlots.Count < ids.Count)
            _spawnedSlots.Add(UIPoolManager.instance.Get(deckSlotPrefab, slotContainer, Vector2.zero));

        while(_spawnedSlots.Count > ids.Count)
        {
            int index = _spawnedSlots.FindIndex(s => !(s.GetComponent<CardDragPayload>()?.IsDragging ?? false));
            if (index < 0) break;

            UIPoolManager.instance.Return(_spawnedSlots[index]);
            _spawnedSlots.RemoveAt(index);
        }

        for(int i = 0; i < ids.Count; i++)
        {
            DeckCardSlotUI slot = _spawnedSlots[i].GetComponent<DeckCardSlotUI>();

            if(slot != null && slot.GetComponent<CardDragPayload>()?.IsDragging == true) continue;
            _spawnedSlots[i].transform.SetSiblingIndex(i);
            CardData data = _cardDatabase != null ? _cardDatabase.FindById(ids[i]) : null;
            slot?.SetUp(this, ids[i], data);
        }
    }

    public void OnSlotClicked(CardData data) => detailPanel?.ShowCard(data);

    public bool RequestAddCard(int cardId)
    {
        if (_session == null) return false;
        bool added = _session.AddCard(cardId);
        if (added)
            _audioManager?.PlaySfx(cardMoveSfxKey);
        else if (!added && _session.IsFull)
        {
            _popupService?.Toast.Show($"덱은 최대 {_session.MaxCards}장 까지 담을 수 있습니다.");
        }
            return added;
    }

    public bool RequestRemoveCard(int cardId)
    {
        if (_session == null) return false;
        bool removed = _session.RemoveOne(cardId);
        if (removed)
            _audioManager?.PlaySfx(cardMoveSfxKey);
        return removed;
    }
}
