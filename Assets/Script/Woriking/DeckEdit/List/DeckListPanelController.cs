using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using VContainer;

public class DeckListPanelController : MonoBehaviour
{
    [SerializeField] private RectTransform gridContainer;
    [SerializeField] private GameObject deckTilePrefab;
    [SerializeField] TextMeshProUGUI countText; // "현재 / 최대"
    [SerializeField] private string countFormat = "{0} / {1}";

    [Header("삭제 모드")]
    [SerializeField] private Button deleteModeToggleButton;
    [SerializeField] private GameObject deleteExecuteBar;
    [SerializeField] private Button deleteExecuteButton;
    [SerializeField] TextMeshProUGUI selectedCountText;

    [Header("씬 이동")]
    [SerializeField] private Button backButton;

    private readonly List<GameObject> _spawnedTiles = new List<GameObject>();
    private readonly HashSet<string> _selectedForDelete = new HashSet<string>();
    private bool _deleteMode;

    private DeckRepository _deckRepository;
    private DeckManagementConfig _config;
    private DeckPopupService _popupService;
    private CardDatabase _cardDatabase;
    private DeckManagementSceneController _sceneController;

    [Inject] 
    public void Construct(DeckRepository deckRepository, DeckManagementConfig config, DeckPopupService popupService, CardDatabase cardDatabase, DeckManagementSceneController sceneController)
    {
        _deckRepository = deckRepository;
        _config = config;
        _popupService = popupService;
        _cardDatabase = cardDatabase;
        _sceneController = sceneController;
    }

    private void Awake()
    {
        deleteModeToggleButton?.onClick.AddListener(ToggleDeleteMode);
        deleteExecuteButton?.onClick.AddListener(OnDeleteExcuteClicked);
        backButton?.onClick.AddListener(() => _sceneController.RequestBack());
        SetDeleteExcuteInteractable();
        deleteExecuteBar?.SetActive(false);
    }

    private void OnEnable()
    {
        if (_deckRepository != null) _deckRepository.OnDecksChanged += Refresh;
    }

    private void OnDisable()
    {
        if (_deckRepository != null) _deckRepository.OnDecksChanged -= Refresh;
    }

    public void Refresh()
    {
        if (_deckRepository == null || gridContainer == null || deckTilePrefab == null) return;
        if (UIPoolManager.instance == null) return;

        var decks = _deckRepository.AllDeckSummaries;
        int slotCount = decks.Count + 1;

        while (_spawnedTiles.Count < slotCount)
            _spawnedTiles.Add(UIPoolManager.instance.Get(deckTilePrefab, gridContainer, Vector2.zero));

        while(_spawnedTiles.Count > slotCount)
        {
            int last = _spawnedTiles.Count - 1;
            UIPoolManager.instance.Return(_spawnedTiles[last]);
            _spawnedTiles.RemoveAt(last);
        }

        // 0번 타일은 + 타일로 고정
        _spawnedTiles[0].transform.SetSiblingIndex(0);
        _spawnedTiles[0].GetComponent<DeckListItemUI>().SetUpAsAddTile(this);

        for(int i = 0; i < decks.Count; i++)
        {
            GameObject tile = _spawnedTiles[i + 1];
            tile.transform.SetSiblingIndex(i + 1);
            var item = tile.GetComponent<DeckListItemUI>();
            item?.Setup(this, decks[i], _cardDatabase);
            item?.SetDeleteMode(_deleteMode, _selectedForDelete.Contains(decks[i].deckId));
        }

        int maxDecks = _config != null ? _config.maxSavedDecks : decks.Count;
        if (countText != null) countText.text = string.Format(countFormat, decks.Count, maxDecks);
    }

    public void OnAddTileClicked()
    {
        if (_deleteMode) return;
        _sceneController?.ShowEdit(null);
    }

    public void OnDeckTileClicked(DeckIndexEntry entry)
    {
        if (_deleteMode || entry == null) return;
        _sceneController?.ShowEdit(entry.deckId);
    }

    public void OnDeckCheckToggled(string deckId, bool isOn)
    {
        if (isOn) _selectedForDelete.Add(deckId);
        else _selectedForDelete.Remove(deckId);

        if (selectedCountText != null) selectedCountText.text = _selectedForDelete.Count.ToString();
        SetDeleteExcuteInteractable();
    }

    public void ToggleDeleteMode()
    {
        _deleteMode = !_deleteMode;
        _selectedForDelete.Clear();
        deleteExecuteBar?.SetActive(_deleteMode);
        if (selectedCountText != null) selectedCountText.text = "0";
        SetDeleteExcuteInteractable();
        Refresh();
    }

    private void SetDeleteExcuteInteractable()
    {
        if (deleteExecuteButton != null)
            deleteExecuteButton.interactable = _deleteMode && _selectedForDelete.Count > 0;
    }


    private void OnDeleteExcuteClicked()
    {
        if (_selectedForDelete.Count == 0) return;

        // 팝업창
        _popupService?.Confirm.Show(
                "정말 삭제하시겠습니까?",
                onConfirm: ExcuteDelete,
                onCancel: null
            );
    }

    private void ExcuteDelete()
    {
        int requested = _selectedForDelete.Count;
        List<string> failed = _deckRepository.DeleteDecks(_selectedForDelete);

        _deleteMode = false;
        _selectedForDelete.Clear();
        deleteExecuteBar?.SetActive(false);
        Refresh();

        if(failed.Count == 0)
        {
            // 팝업창
            _popupService?.Toast.ShowSuccess("삭제되었습니다.");
        }
        else if(failed.Count < requested)
        {
            // 팝업창
            _popupService?.Confirm.ShowAlert("오류", $"덱 {failed.Count}개를 삭제하지 못했습니다. 다시 시도해 주세요"); ;
        }
        else
        {
            // 팝업창
            _popupService?.Confirm.ShowAlert("오류", "덱을 삭제하지 못했습니다. 다시 시도해 주세요");
        }
    }
}
