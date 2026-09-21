using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;


// 저장된 덱들의 CRUD를 담당하는 영구 매니저
public class DeckRepository : MonoBehaviour, IInitializable
{
    public static DeckRepository Instance {  get; private set; }

    [SerializeField] private DeckManagementConfig config;

    [Header("최초 지급 덱(저장된 덱이 하나도 없을 때 실행")]
    [SerializeField] private string starterDeckName = "기본 덱";
    [SerializeField] private List<int> starterDeckCardIDS = new List<int>();

    private SaveManager _saveManager;
    private DeckSaveManager _deckSaveManager;

    // 덱 리스트 화면에서는 이 요약 정보만 씀
    public IReadOnlyList<DeckIndexEntry> AllDeckSummaries => (IReadOnlyList<DeckIndexEntry>)_deckSaveManager?.index.entries ?? Array.Empty<DeckIndexEntry>();
    public string ActiveDeckId => _deckSaveManager?.index.activeDeckId ?? "";

    public event Action OnDecksChanged;

    [Inject] 
    public void Construct(SaveManager saveManager)
    {
        _saveManager = saveManager;
    }

    private void Awake()
    {
        Debug.Log("[DeckRepository] Awake 호출됨");
        Instance = this;
    }

    // VContainer 엔트리포인트 디스패치 타이밍에 안 걸릴 수 있어서 실제 사용 시점에서 한번 더 보장되게끔 방어하는 코드
    public void Initialize() => EnsureReady();

    private void EnsureReady()
    {
        if (_deckSaveManager != null) return;

        int maxSlots = config != null ? config.maxSavedDecks : 5;
        bool isFirstRun = !DeckSaveManager.HasExistingIndex(_saveManager);
        _deckSaveManager = new DeckSaveManager(_saveManager, maxSlots);

        if (isFirstRun) GrantStarterDeck();
    }

 
    public bool CanCreateNewDeck() => _deckSaveManager != null && _deckSaveManager.CanCreateNewDeck();

    public string CreateNewDeckId() => _deckSaveManager != null ? _deckSaveManager.CreateNewDeckId() : Guid.NewGuid().ToString("N");

    public DeckData LoadDeck(string deckId)
    {
        if(string.IsNullOrEmpty(deckId) || _deckSaveManager == null)  return null;
        return _deckSaveManager.LoadDeck(deckId);
    }

    public bool SaveDeck(DeckData deck, out string errorMessage)
    {
        EnsureReady();
        if(_deckSaveManager == null)
        {
            errorMessage = "저장소가 초기화되지 않았습니다.";
            return false;
        }
        if(deck == null)
        {
            errorMessage = "저장할 덱 정보가 없습니다.";
            return false;
        }
        if(config != null && deck.cardIds.Count > config.maxCardsPerDeck)
        {
            errorMessage = $"덱은 최대 {config.maxCardsPerDeck}장까지 저장할 수 있습니다.";
            return false;
        }

        bool success = _deckSaveManager.SaveDeck(deck, out errorMessage);
        if (success) OnDecksChanged?.Invoke();
        return success;
    }

    public List<string> DeleteDecks(IEnumerable<string> deckIds)
    {
        if (_deckSaveManager == null) return new List<string>(deckIds ?? Array.Empty<string>());
        List<string> failed = _deckSaveManager.DeleteDecks(deckIds);
        OnDecksChanged?.Invoke();
        return failed;
    }

    public void SetActiveDeck(string deckId)
    {
        if (_deckSaveManager != null && _deckSaveManager.SetActiveDeck(deckId))
            OnDecksChanged?.Invoke();
    }

    private void GrantStarterDeck()
    {
        if (starterDeckCardIDS.Count == 0) return;

        DeckData starterDeck = new DeckData(_deckSaveManager.CreateNewDeckId(), starterDeckName)
        {
            cardIds = new List<int>(starterDeckCardIDS)
        };
        if (!_deckSaveManager.SaveDeck(starterDeck, out string errorMessage))
            Debug.LogWarning($"[DeckRepository] 기본 덱 자동 생성 실패 : {errorMessage}");
    }
}
