using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class OwnedCardListController : MonoBehaviour
{
    [SerializeField] private RectTransform slotContainer;
    [SerializeField] private GameObject ownedSlotPrefab;
    [SerializeField] private CardDetailPanelController detailPanel;
    [SerializeField] private DeckGridController deckGrid;

    private readonly List<GameObject> _spawnedSlots = new List<GameObject>();
    private DeckEditSession _session;
    private PlayerCardCollection _cardCollection;
    private CardDatabase _cardDatabase;

    [Inject]
    public void Construct(PlayerCardCollection cardCollection, CardDatabase cardDatabase)
    {
        _cardCollection = cardCollection;
        _cardDatabase = cardDatabase;
    }

    public void Bind(DeckEditSession session)
    {
        if (_session != null) _session.OnChanged -= Rebuild;
        _session = session;
        _session.OnChanged += Rebuild;
        if (_cardCollection != null) _cardCollection.OnCollectionChanged += Rebuild;
        Rebuild();
    }

    private void OnDestroy()
    {
        if (_session != null) _session.OnChanged -= Rebuild;
        if (_cardCollection != null) _cardCollection.OnCollectionChanged -= Rebuild;
    }

    private void Rebuild()
    {
        if(_session == null || _cardCollection == null || _cardDatabase == null) return;

        var visible = new List<(CardData data, int remaining)>();
        foreach (var kvp in _cardCollection.GetAllOwned())
        {
            if (kvp.Value <= 0) continue;

            CardData data = _cardDatabase.FindById(kvp.Key);
            if (data == null) continue;

            int remaining = _session.GetRemainingOwned(kvp.Key);

            visible.Add((data, remaining));
        }

        while (_spawnedSlots.Count < visible.Count)
            _spawnedSlots.Add(UIPoolManager.instance.Get(ownedSlotPrefab, slotContainer, Vector2.zero));

        while(_spawnedSlots.Count > visible.Count)
        {
            int index = _spawnedSlots.FindIndex(s => !(s.GetComponent<CardDragPayload>()?.IsDragging ?? false));
            if (index < 0) break;

            UIPoolManager.instance.Return(_spawnedSlots[index]);
            _spawnedSlots.RemoveAt(index);
        }

        for(int i = 0; i < visible.Count; i++)
        {
            var (data, remaining) = visible[i];
            OwnedCardSlotUI slot = _spawnedSlots[i].GetComponent<OwnedCardSlotUI>();

            if (slot != null && slot.GetComponent<CardDragPayload>()?.IsDragging == true) continue;

            _spawnedSlots[i].transform.SetSiblingIndex(i);
            slot?.SetUp(this, data, remaining);
        }
    }

    public void OnSlotClicked(CardData data) => detailPanel?.ShowCard(data);

    public bool RequestAddCard(int cardId) => deckGrid != null && deckGrid.RequestAddCard(cardId);
}
