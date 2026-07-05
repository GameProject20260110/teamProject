using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager instance;

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


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
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
        }
            
        foreach (var item in battleItems)
        {
            item.inventoryIconRect = inventoryIconRect;
            item.ShopCanvas = ShopCanvas;
        }          

        ReRoll();
    }

    public void ReRoll()
    {
        if (PlayerShopManager.instance.RerollCount > 0)
        {
            bool success = PlayerShopManager.instance.TryReroll();
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
        await PlayerShopManager.instance.CommitWithAnimation();
        SceneController.instance.LoadMapScene();
    }
}