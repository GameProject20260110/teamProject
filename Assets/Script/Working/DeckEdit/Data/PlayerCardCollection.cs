using UnityEngine;
using System.Collections.Generic;
using System;
using VContainer;
using VContainer.Unity;

public class PlayerCardCollection : MonoBehaviour, IInitializable
{

    [Header("디버그 - 테스트용(추후에 삭제해야 함")]
    [SerializeField] private int debugCardId;
    [SerializeField] private int debugAmount = 1;

    [ContextMenu("입력한 카드 지급")]
    private void DebugAddCard()
    {
        AddCard(debugCardId, debugAmount);
        Debug.Log($"[디버그] cardId {debugCardId} {debugAmount}장 지급됨");
    }

    public static PlayerCardCollection Instance { get; private set; }

    [Header("최초 지급 카드")]
    [SerializeField] private List<CardCountEntry> defaultOwnedCards = new List<CardCountEntry>();

    private const string SAVE_FILE = "cardCollection.json";

    private readonly Dictionary<int, int> _ownedCounts = new Dictionary<int, int>();
    private SaveManager _saveManager;

    public event Action OnCollectionChanged;

    [Inject]
    public void Construct(SaveManager saveManager)
    {
        _saveManager = saveManager;
        Instance = this;
    }

    public void Initialize()
    {
        Debug.Log($"[PlayerCardCollection] Initialize 호출됨!");
        if (_saveManager != null && _saveManager.HasSaveFile(SAVE_FILE))
            Load();
        else
            ResetToDefault();
    }

    public int GetOwnedCount(int cardId)
    {
        return _ownedCounts.TryGetValue(cardId, out int count) ? count : 0;
    }

    public void AddCard(int cardId, int amount = 1)
    {
        if (amount <= 0) return;
        _ownedCounts.TryGetValue(cardId, out int current);
        _ownedCounts[cardId] = current + amount;
        Save();
        OnCollectionChanged.Invoke();
    }

    public bool RemoveCard(int cardId, int amount = 1)
    {
        if (amount <= 0) return false;
        if (!_ownedCounts.TryGetValue(cardId, out int current) || current <= 0) return false;

        int next = Mathf.Max(0, current - amount);
        if (next == 0) _ownedCounts.Remove(cardId);
        else _ownedCounts[cardId] = next;

        Save();
        OnCollectionChanged?.Invoke();
        return true;
    }

    public IReadOnlyDictionary<int, int> GetAllOwned() => _ownedCounts;

    private void ResetToDefault()
    {
        _ownedCounts.Clear();
        foreach(var entry in defaultOwnedCards)
        {
            if (entry.count <= 0) continue;
            _ownedCounts[entry.cardId] = entry.count;
        }
        Save();
    }

    private void Save()
    {
        if (_saveManager == null) return;
        var data = new CardCollectionSaveData();
        foreach (var kvp in _ownedCounts)
            data.entries.Add(new CardCountEntry(kvp.Key, kvp.Value));
        if (!_saveManager.Save(data, SAVE_FILE))
            Debug.LogWarning("[PlayerCardCollection] 보유 카드 저장 실패");
    }

    private void Load()
    {
        _ownedCounts.Clear();
        if(!_saveManager.Load(SAVE_FILE, out CardCollectionSaveData data) || data?.entries == null)
        {
            Debug.LogWarning("[PlayerCardCollection] 보유 카드 데이터를 불러오지 못함");
            return;
        }

        foreach(var entry in data.entries)
        {
            if (entry.count <= 0) continue;
            _ownedCounts[entry.cardId] = entry.count;
        }
    }

    [Serializable]
    private class CardCollectionSaveData 
    {
        public List<CardCountEntry> entries = new List<CardCountEntry>();
    }
}
