using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ShopUIManager : MonoBehaviour
{
    [Header("Shop Items")]
    [SerializeField] private ShopDiceItem[] diceItems;
    [SerializeField] private ShopBattleItem[] battleItems;

    [Header("Gacha Tables")]
    [SerializeField] private DiceGachaDatabase ShopDiceDatabase;
    [SerializeField] private ItemGachaTable itemGacha;

    [Header("UI")]
    [SerializeField] private Button exitButton;

    [Header("인벤토리")]
    [SerializeField] private RectTransform inventoryIconRect;
    [SerializeField] private Transform ShopCanvas;

    private SceneController _sceneController;
    private PlayerShopManager _playerShopManager;
    private PopupManager _popupManager;

    [Inject]
    public void Construct(SceneController sceneController, PlayerShopManager playerShopManager, PopupManager popupManager)
    {
        _sceneController = sceneController;
        _playerShopManager = playerShopManager;
        _popupManager = popupManager;
    }

    private void Start()
    {
        exitButton.onClick.AddListener(() =>
        {
            OnExitClicked().Forget();
        });
    }

    public void Initialize()
    {
        // 인벤토리 아이콘 참조 연결
        foreach (var item in diceItems)
        {
            item.inventoryIconRect = inventoryIconRect;
            item.ShopCanvas = ShopCanvas;
            item.SetDependencies(_playerShopManager, _popupManager);
        }
            
        foreach (var item in battleItems)
        {
            item.inventoryIconRect = inventoryIconRect;
            item.ShopCanvas = ShopCanvas;
            item.SetDependencies(_playerShopManager, _popupManager);
        }          
        ReRoll();
    }

    public void ReRoll()
    {
        if (_playerShopManager.RerollCount > 0)
        {
            bool success = _playerShopManager.TryReroll();
            if (!success) return;
        }

        RerollDice();
        RerollItem();
    }

    private void RerollDice()
    {
        var dicega = ShopDiceDatabase.diceGachaList[0];
        foreach (var item in diceItems)
            item.Setup(dicega.Roll());
    }

    private void RerollItem()
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