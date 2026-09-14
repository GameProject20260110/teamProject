using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

// 덱 관리 씬의 루트 컨트롤러
public class DeckManagementSceneController : MonoBehaviour
{
    public static DeckManagementSceneController Instance { get; private set; }

    [SerializeField] private CanvasGroup rootCanvasGroup;

    [Header("화면 패널")]
    [SerializeField] private DeckListPanelController listPanel;
    [SerializeField] private DeckEditScreenController editPanel;


    private DeckEditSession _editSession;
    private DeckPopupService _popupService;
    private DeckManagementConfig _config;

    private RectTransform _listRect;
    private RectTransform _editRect;
    private CanvasGroup _listGroup;
    private CanvasGroup _editGroup;
    private bool _isPanelTransitioning;

    private bool IsEditActive => editPanel != null && editPanel.gameObject.activeSelf;

    [Inject] 
    public void Construct(DeckEditSession editSession, DeckPopupService popupService, DeckManagementConfig config)
    {
        _editSession = editSession;
        _popupService = popupService;
        _config = config;
    }

    private void Awake()
    {
        Instance = this;
        if(rootCanvasGroup != null)
        {
            rootCanvasGroup.alpha = 0f;
            rootCanvasGroup.blocksRaycasts = false;
        }

        if(listPanel != null)
        {
            _listRect = (RectTransform)listPanel.transform;
            _listGroup = listPanel.GetComponent<CanvasGroup>() ?? listPanel.gameObject.AddComponent<CanvasGroup>();
        }
        if(editPanel != null)
        {
            _editRect = (RectTransform)editPanel.transform;
            _editGroup = editPanel.GetComponent<CanvasGroup>() ?? editPanel.gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        ShowListImmediate();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            RequestBack();
    }

    public void RequestBack()
    {
        if (_isPanelTransitioning) return;

        if (_popupService != null && _popupService.IsAnyPopupOpen) return;

        if (IsEditActive)
            RequestBackToList();
        else
            RequestCloseToScene();
        
    }

    public async UniTask PlayOpenAsync()
    {
        if (rootCanvasGroup == null) return;
        rootCanvasGroup.blocksRaycasts = true;
        float duration = _config != null ? _config.sceneFadeDuration : 0.25f;
        await rootCanvasGroup.DOFade(1f, duration).SetEase(Ease.OutQuad).SetUpdate(true).ToUniTask();
    }

    public async UniTask PlayCloseAsync()
    {
        if (rootCanvasGroup == null) return;
        rootCanvasGroup.blocksRaycasts = false;
        float duration = _config != null ? _config.sceneFadeDuration : 0.25f;
        await rootCanvasGroup.DOFade(0f, duration).SetEase(Ease.InQuad).SetUpdate(true).ToUniTask();
    }

    public void ShowListImmediate()
    {
        if (_listRect != null) _listRect.anchoredPosition = Vector2.zero;
        if (_editRect != null) _editRect.anchoredPosition = Vector2.zero;
        listPanel?.gameObject.SetActive(true);
        editPanel?.gameObject.SetActive(false);
        listPanel?.Refresh();
    }

    public void ShowList()
    {
        if (_isPanelTransitioning || !IsEditActive) return;
        SlidePanelAsync(
            fromObj: editPanel.gameObject, fromRect: _editRect, fromGroup: _editGroup,
            toObj: listPanel.gameObject, toRect: _listRect, toGroup: _listGroup,
            enterFromSign: -1f,
            onTargetReady: () => listPanel?.Refresh()
        ).Forget();
    }

    public void ShowEdit(string deckId)
    {
        if (_isPanelTransitioning || IsEditActive) return;
        SlidePanelAsync(
            fromObj: listPanel.gameObject, fromRect: _listRect, fromGroup: _listGroup,
            toObj: editPanel.gameObject, toRect: _editRect, toGroup: _editGroup,
            enterFromSign: 1f,
            onTargetReady: () => editPanel?.Open(deckId)
        ).Forget();
    }

    private async UniTask SlidePanelAsync(GameObject fromObj, RectTransform fromRect, CanvasGroup fromGroup, GameObject toObj, RectTransform toRect, CanvasGroup toGroup, float enterFromSign, System.Action onTargetReady)
    {
        if (fromRect == null || toRect == null || rootCanvasGroup == null)
        {
            fromObj?.SetActive(false);
            toObj?.SetActive(true);
            onTargetReady?.Invoke();
            return;
        }

        _isPanelTransitioning = true;
        if (fromGroup != null) fromGroup.blocksRaycasts = false;
        if(toGroup != null) toGroup.blocksRaycasts = false;

        float distance = ((RectTransform)rootCanvasGroup.transform).rect.width;
        float duration = _config != null ? _config.panelSlideDuration : 0.25f;

        toRect.DOKill();
        fromRect.DOKill();
        toRect.anchoredPosition = new Vector2(distance * enterFromSign, 0f);

        toObj?.SetActive(true);
        onTargetReady?.Invoke();

        UniTask fromTween = fromRect.DOAnchorPos(new Vector2(-distance * enterFromSign, 0f), duration).SetEase(Ease.InOutQuad).ToUniTask();
        UniTask toTween = toRect.DOAnchorPos(Vector2.zero, duration).SetEase(Ease.InOutQuad).ToUniTask();
        await UniTask.WhenAll(fromTween, toTween);

        fromObj.SetActive(false);
        fromRect.anchoredPosition = Vector2.zero;

        if (fromGroup != null) fromGroup.blocksRaycasts = true;
        if (toGroup != null) toGroup.blocksRaycasts = true;
        _isPanelTransitioning = false;

    }

    public void RequestCloseToScene()
    {
        DeckSceneLoader.CloseAsync().Forget();
    }

    public void RequestBackToList()
    {
        if(_editSession == null || !_editSession.IsDirty())
        {
            ShowList();
            return;
        }

        _popupService?.Confirm.Show(
            "저장하시겠습니까?",
            onConfirm: () =>
            {
                editPanel?.SaveCurrentDeck();
                ShowList();
            },
            onCancel: ShowList,
            confirmLabel: "저장",
            cancelLabel: "취소"
        );
    }
}
