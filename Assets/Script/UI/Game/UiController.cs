using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class UiController : MonoBehaviour
{
    public static UiController Instance;
    [Header("모듈")]
    public ItemInventoryUI inventoryUI;
    public ResultPanelUI resultUI;
    public GameOverPanelUI gameOverUI;

    [Header("인게임 정보 UI (상시 표시)")]
    public TextMeshProUGUI goldText;

    [Header("버튼")]
    public Button rollBtn;
    public Button confirmBtn;

    [Header("GameEndPanels")]
    public GameObject backGround;
    public GameObject InventoryPanel;
    public GameObject itemDarkPanel;
    public GameObject DropZone;

    [SerializeField] private Sprite clickBtn;
    [SerializeField] private Sprite NoclickBtn;

    private GameManager _gameManager;

    [Inject]
    public void Construct(GameManager gameManager)
    {
        _gameManager = gameManager;
        Instance = this;
    }

    private void Start()
    {
        if (_gameManager != null)
        {
            SubscribeToEvents();
        }
        RefreshInventory();
    }

    private void OnDisable()
    {
        if (_gameManager != null) UnSubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _gameManager.OnGoldChanged += UpdateGoldUi;
    }

    private void UnSubscribeToEvents()
    {
        _gameManager.OnGoldChanged -= UpdateGoldUi;
    }

    private void UpdateGoldUi(int gold)
    {
        if (goldText != null)
        {
            goldText.text = gold.ToString("N0");
        }
    }

    public void ToggleItemDragPanel()
    {
        itemDarkPanel.SetActive(!itemDarkPanel.activeSelf);
        DropZone.SetActive(!DropZone.activeSelf);
    }

    public void ToggleInventoryPanel()
    {
        InventoryPanel.SetActive(!InventoryPanel.activeSelf);
    }

    public void RefreshInventory()
    {
        inventoryUI?.Refresh();
    }

    public void HideAllPanels()
    {
        resultUI?.Hide();
        gameOverUI?.Hide();
    }

    public void ShowGlowImage() => confirmBtn.GetComponent<ButtonGlowController>().ShowImageGlow();
    public void HideGlowImage() => confirmBtn.GetComponent<ButtonGlowController>().HideImageGlow();
    public void ShowGlowShader() => confirmBtn.GetComponent<ButtonGlowController>().ShowShaderGlow();
    public void HideGlowShader() => confirmBtn.GetComponent<ButtonGlowController>().HideShaderGlow();
    public void ShowGlow() => confirmBtn.GetComponent<ButtonGlowController>().ShowGlow();
    public void HideGlow() => confirmBtn.GetComponent<ButtonGlowController>().HideGlow();

    public void ShowResultPanel(bool isSuccess, int currentLife)
    {
        resultUI?.Show(isSuccess, currentLife);
        RefreshInventory();
    }

    public void ShowGameOverPanel(int round)
    {
        gameOverUI?.Show(round);
    }

    public void SetRollBtnInteractable(bool state)
    {
        if (rollBtn != null) rollBtn.interactable = state;
    }

    public void SetConfirmBtnInteratable(bool state)
    {
        if (confirmBtn != null) confirmBtn.interactable = state;
    }

    public void OnClickGameEndBtn() => gameOverUI?.Show(12);

    public void NegateItemCard(string itemName, GameObject negateOverlayPrefab)
    {
        var card = inventoryUI?.FindCardByName(itemName);
        if (card != null) card.PlayNegateEffect(negateOverlayPrefab);
    }

    public void ResetItemCards() => inventoryUI?.ResetCards();

    public void OnClickConfirmBtn() => confirmBtn.image.sprite = clickBtn;
    public void OffClickConfirmBtn() => confirmBtn.image.sprite = NoclickBtn;
}
