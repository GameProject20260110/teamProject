using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class DeckEditScreenController : MonoBehaviour
{
    [Header("상단 바")]
    [SerializeField] private TextMeshProUGUI deckNameText;
    [SerializeField] private Button renameButton;
    [SerializeField] private TextMeshProUGUI cardCountText;
    [SerializeField] private string cardCountFormat = "{0}/{1}";

    [Header("버튼")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button backButton;

    [Header("연결 패널")]
    [SerializeField] private DeckGridController gridController;
    [SerializeField] private OwnedCardListController ownedListController;
    [SerializeField] private CardDetailPanelController detailPanel;

    private DeckEditSession _session;
    private DeckRepository _repository;
    private DeckManagementSceneController _sceneController;
    private DeckPopupService _popupService;
    private DeckManagementConfig _config;

    [Inject]
    public void Construct(DeckEditSession session, DeckRepository repository, DeckManagementSceneController sceneController, DeckPopupService popupService, DeckManagementConfig config)
    {
        _session = session;
        _repository = repository;
        _sceneController = sceneController;
        _popupService = popupService;
        _config = config;
    }

    private void Awake()
    {
        renameButton.onClick.AddListener(OnRenameClicked);
        saveButton.onClick.AddListener(OnSaveClicked);
        backButton.onClick.AddListener(() => _sceneController.RequestBack());
    }

    private void OnEnable()
    {
        if (_session != null) _session.OnChanged += Refresh;
    }

    private void OnDisable()
    {
        if(_session != null) _session.OnChanged -= Refresh;
    }

    public void Open(string deckId)
    {
        DeckData deck = null;
        if(!string.IsNullOrEmpty(deckId))
        {
            deck = _repository.LoadDeck(deckId);
            if (deck == null)
                _popupService?.Confirm.ShowAlert("알림", "덱 정보를 불러오지 못했습니다.");
        }

        _session.Load(deck);
        gridController?.Bind(_session);
        ownedListController?.Bind(_session);
        detailPanel?.Clear();
        Refresh();
    }

    private void Refresh()
    {
        if (deckNameText != null)
            deckNameText.text = string.IsNullOrEmpty(_session.DeckName) ? (_config != null ? _config.defaultDeckName : "새 덱") : _session.DeckName;
        if(cardCountText != null) 
            cardCountText.text = string.Format(cardCountFormat, _session.SlotCardIds.Count, _session.MaxCards);
    }

    private void OnRenameClicked()
    {
        _popupService?.NameInput.Show(_session.DeckName, newName => _session.Rename(newName));
    }

    private void OnSaveClicked()
    {
        SaveCurrentDeck();
    }

    public void SaveCurrentDeck()
    {
        if (_session == null || _repository == null) return;

        DeckData deck = _session.BuildDeckForSave();
        bool success = _repository.SaveDeck(deck, out string errorMessage);

        if(success)
        {
            _session.MarkSaved();
            saveButton?.gameObject.SetActive(false);
            backButton?.gameObject.SetActive(false);
            _popupService.Toast.ShowSuccess("저장했습니다.", () =>
            {
                saveButton?.gameObject.SetActive(true);
                backButton?.gameObject.SetActive(true);
                _sceneController?.RequestBack();
            });
            _sceneController?.RequestBack();
        }
        else
        {
            _popupService.Confirm.ShowAlert("저장 실패", errorMessage);
        }
    }
}
