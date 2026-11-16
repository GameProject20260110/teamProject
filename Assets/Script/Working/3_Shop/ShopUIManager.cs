using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;

public class ShopUIManager : MonoBehaviour
{
    [Header("Shop Items")]
    [FormerlySerializedAs("diceItems")]
    [SerializeField] private ShopCardItem[] cardItems;
    [SerializeField] private ShopBattleItem[] battleItems;

    [Header("Gacha Tables")]
    [SerializeField] private CardGachaDatabase cardGachaDatabase;
    [SerializeField] private ItemGachaTable itemGacha;

    [Header("UI")]
    [SerializeField] private Button exitButton;
    [SerializeField] private Transform ShopCanvas;

    [Header("Sound")]
    [SerializeField] private string purchaseSfxKey;

    private RectTransform inventoryIconRect;

    private SceneController _sceneController;
    private PlayerShopManager _playerShopManager;
    private ShopSession _session;
    private PopupManager _popupManager;
    private PlayerHudManager _playerHudManager;
    private AudioManager _audioManager;

    [Inject]
    public void Construct(SceneController sceneController, PlayerShopManager playerShopManager,
        ShopSession session, PopupManager popupManager,
        PlayerHudManager playerHudManager, AudioManager audioManager)
    {
        _sceneController = sceneController;
        _playerShopManager = playerShopManager;
        _session = session;
        _popupManager = popupManager;
        _playerHudManager = playerHudManager;
        _audioManager = audioManager;
    }

    private void Start()
    {
        exitButton.onClick.AddListener(() => OnExitClicked().Forget());
    }

    public void Initialize()
    {
        inventoryIconRect = _playerHudManager.Inventory.GetComponent<RectTransform>();

        foreach (var item in cardItems)
        {
            if (item == null) continue;
            SetupSlot(item);
            item.Bind(
                onBuy: card => PlayIfSuccess(_session.TryPurchaseCard(card)),
                onHoverEnter:   card => _popupManager.OpenPopup(card),
                onHoverExit: _ => _popupManager.ClosePopup());
        }

        foreach (var item in battleItems)
        {
            if (item == null) continue;
            SetupSlot(item);
            item.Bind(onBuy: data => PlayIfSuccess(_session.TryPurchaseItem(data)));
        }

        ReRoll();
    }

    private void SetupSlot<T>(ShopItem<T> item) where T : class
    {
        item.inventoryIconRect = inventoryIconRect;
        item.ShopCanvas = ShopCanvas;
    }

    private bool PlayIfSuccess(bool success)
    {
        if (success) _audioManager.PlaySfx(purchaseSfxKey);
        return success;
    }

    public void ReRoll()
    {
        if (_session.RerollCount > 0 && !_session.TryReroll()) return;

        RerollCards();
        RerollItems();
    }

    private void RerollCards()
    {
        var table = cardGachaDatabase.GetTable(0);
        
        // 중복 허용 뽑기
        foreach(var slot in cardItems)
        {
            var card = table.Roll();
            if (card != null) slot.Setup(card);
            else slot.Hide();
        }

        // 중복 제거 뽑기
        //var cards = table.RollDistinct(cardItems.Length);

        //for (int i = 0; i < cardItems.Length; i++)
        //{
        //    if (i < cards.Count) cardItems[i].Setup(cards[i]);
        //    else cardItems[i].Hide();
        //}       
    }

    private void RerollItems()
    {
        foreach (var item in battleItems)
            item.Setup(itemGacha.Roll());
    }

    private async UniTaskVoid OnExitClicked()
    {
        exitButton.interactable = false;
        await _playerShopManager.CommitWithAnimation();
        _sceneController.LoadMapScene();
    }
}