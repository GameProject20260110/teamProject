using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    public static UiController instance = null;

    [Header("모듈")]
    public ItemInventoryUI inventoryUI;
    public ResultPanelUI resultUI;
    public GameOverPanelUI gameOverUI;
    public GimmickUI gimmickUI;
    public NotificationUI notificationUI;

    [Header("인게임 정보 UI (상시 표시)")]
    public TextMeshProUGUI roundInfoText;
    public TextMeshProUGUI targetScoreInfoText;
    public TextMeshProUGUI myScoreInfoText;
    public TextMeshProUGUI goldText;

    [Header("버튼")]
    public Button rollBtn;
    public TextMeshProUGUI rerollText;
    public Image rollBtnImage;
    public Sprite rollSprite;
    public Sprite rerollSprite;
    public Button confirmBtn;
    public Button ShopBtn;

    [Header("GameEndPanels")]
    public GameObject backGround;

    [SerializeField] private Image _playerImage;

    [SerializeField] private Sprite clickBtn;
    [SerializeField] private Sprite NoclickBtn;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {

        if(GameManager.instance != null)
        {
            SubscribeToEvents();
        }
        if(notificationUI != null)
        {
            notificationUI.gameObject.SetActive(false);
        }
        RefreshInventory();

        if(PlayerManager.instance != null && _playerImage != null)
        {
            _playerImage.sprite = PlayerManager.instance.playerImage;
        }
    }

    private void OnDisable()
    {
        if (GameManager.instance != null) UnSubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        GameManager.instance.OnGoldChanged += UpdateGoldUi;
        GameManager.instance.OnRoundAndGoalChanged += UpdateRoundAndGoalUi;
        GameManager.instance.OnRerollCountChanged += UpdateRerollUi;
    }

    private void UnSubscribeToEvents()
    {
        GameManager.instance.OnGoldChanged -= UpdateGoldUi;
        GameManager.instance.OnRoundAndGoalChanged -= UpdateRoundAndGoalUi;
        GameManager.instance.OnRerollCountChanged -= UpdateRerollUi;
    }

    private void UpdateGoldUi(int gold)
    {
        if(goldText != null)
        {
            goldText.text = gold.ToString("N0");
        }
    }


    private void UpdateRoundAndGoalUi(int round)
    { 
        if (roundInfoText != null)
        {
            roundInfoText.SetText("{0}", round);
        }
    }

    public void RefreshGimmickIcons(List<GimmickSo> gimmick)
    {
        gimmickUI?.RefreshIcons(gimmick);
    }

    public void SetRollButtonToRoll()
    {
        if(rollBtnImage != null && rollSprite != null)
        {
            rollBtnImage.sprite = rollSprite;
        }
    }

    public void SetRollButtnonToReroll()
    {
        if(rollBtnImage != null && rerollSprite != null)
        {
            rollBtnImage.sprite = rerollSprite;
        }
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

    public void ShowGlowConfirmBtn()
    {
        confirmBtn.GetComponent<ButtonGlowController>().ShowGlow();
    }

    public void HideGlowConfirmBtn()
    {
        confirmBtn.GetComponent<ButtonGlowController>().HideGlow();
    }

    public void ShowResultPanel(bool isSuccess, int currentLife)
    {
        resultUI?.Show(isSuccess, currentLife);
        RefreshInventory();
    }

    public void ShowGameOverPanel(int round)
    {
        gameOverUI?.Show(round);
    }

    public void UpdateRerollUi(int count)
    {
        if (rerollText != null)
            rerollText.SetText($"{count}/{PlayerManager.instance?.gameRerollCount}");
    }

    public void SetRollBtnInteractable(bool state)
    {
        if(rollBtn != null)
        {
            rollBtn.interactable = state;
        }
    }

    public void SetConfirmBtnInteratable(bool state)
    {
        if (confirmBtn != null)
        {
            confirmBtn.interactable = state;
        }
    }

    public void SetShopBtnInteratable(bool state)
    {
        if(ShopBtn != null)
        {
            ShopBtn.interactable = state;
        }
    }

    public void OnClickGameEndBtn()
    {
        gameOverUI?.Show(12);
    }

    public void ClearGimmickIcons()
    {
        gimmickUI?.ClearIcons();
    }

    public void GotoLobby()
    {
        SceneController.instance.LoadHomeScene();
    }

    public void NegateItemCard(string itemName, GameObject negateOverlayPrefab)
    {
        var card = inventoryUI?.FindCardByName(itemName);
        if (card != null) card.PlayNegateEffect(negateOverlayPrefab);
    }
    public void ResetItemCards()
    {
        inventoryUI?.ResetCards();
    }

    public void OnClickConfirmBtn()
    {
        confirmBtn.image.sprite = clickBtn;
    }

    public void OffClickConfirmBtn()
    {
        confirmBtn.image.sprite = NoclickBtn;
    }

}
