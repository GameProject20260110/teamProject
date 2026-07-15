using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class UiController : MonoBehaviour
{
    public static UiController Instance;
    [Header("모듈")]
    public ItemInventoryUI inventoryUI;
    public GameOverPanelUI gameOverUI;

    [Header("버튼")]
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
        RefreshInventory();
    }

    public void ToggleItemDragPanel()
    {
        itemDarkPanel.SetActive(!itemDarkPanel.activeSelf);
        DropZone.SetActive(!DropZone.activeSelf);
    }

    public void RefreshInventory()
    {
        inventoryUI?.Refresh();
    }

    public void HideAllPanels()
    {
        gameOverUI?.Hide();
    }

    public void ShowGlowImage() => confirmBtn.GetComponent<ButtonGlowController>().ShowImageGlow();
    public void HideGlowImage() => confirmBtn.GetComponent<ButtonGlowController>().HideImageGlow();
    public void ShowGlowShader() => confirmBtn.GetComponent<ButtonGlowController>().ShowShaderGlow();
    public void HideGlowShader() => confirmBtn.GetComponent<ButtonGlowController>().HideShaderGlow();
    public void ShowGlow() => confirmBtn.GetComponent<ButtonGlowController>().ShowGlow();
    public void HideGlow() => confirmBtn.GetComponent<ButtonGlowController>().HideGlow();

    public void ShowGameOverPanel(bool isWin)
    {
        gameOverUI?.Show(isWin);
    }

    public void SetConfirmBtnInteratable(bool state)
    {
        if (confirmBtn != null) confirmBtn.interactable = state;
    }

    public void ResetItemCards() => inventoryUI?.ResetCards();
}
