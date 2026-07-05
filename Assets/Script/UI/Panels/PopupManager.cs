using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public static PopupManager instance;

    [Header("주사위 팝업")]
    public RectTransform dicePopup;
    private TextMeshProUGUI diceDesc;
    [SerializeField] private Image diceIcon;
    [SerializeField] private TextMeshProUGUI diceName;

    [Header("아이템 팝업")]
    public RectTransform itemPopup;
    private TextMeshProUGUI itemDesc;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;

    [Header("홀딩 안내")]
    [SerializeField] private GameObject holdHint;

    [Header("설명 팝업")]
    public ShopDescPopup DescPopup;

    [Header("플레이어 정보")]
    public TextMeshProUGUI playerGold;

    [SerializeField] private GameObject hoverHintText;

    [Header("기타")]
    //public GameObject closePanel;
    public Button StartBtn;
    public Button SettingsBtn;
    public Canvas rootCanvas;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if(dicePopup != null)
        {
            diceDesc = dicePopup.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if(itemPopup != null)
        {
            itemDesc = itemPopup.GetComponentInChildren<TextMeshProUGUI>();
        }      
    }

    private void Start()
    {
        ClosePopup();
        if (hoverHintText != null) hoverHintText.SetActive(true);
        if (playerGold != null)
        {
            int gold = PlayerShopManager.instance != null && PlayerShopManager.instance.IsOpen ? 
                PlayerShopManager.instance.TempGold : ResourceManager.instance.gold;
            playerGold.text = gold.ToString();
        }

        if (PlayerShopManager.instance != null)
        {
            PlayerShopManager.instance.OnGoldChanged += UpdateGold;
        }

        //if (StartBtn != null) 
        //    StartBtn.onClick.AddListener(() => PlayerShopManager.instance.CommitWithAnimation());           

        if(SettingsBtn != null)
            SettingsBtn.onClick.AddListener(() => MainOption.instance.ToggleSettingsPanel());           
    }

    private void OnDestroy()
    {
        if (PlayerShopManager.instance != null)
            PlayerShopManager.instance.OnGoldChanged -= UpdateGold;
    }

    public void SetStatus()
    {
        playerGold.text = ResourceManager.instance.gold.ToString();
    }

    public void DescOpenPopup(DiceData data)
    {
        DescPopup.gameObject.SetActive(true);
        DescPopup.UpdateUI(data);
    }

    public void DescOpenPopup(BattleItemSo data)
    {
        DescPopup.gameObject.SetActive(true);
        DescPopup.UpdateUI(data);
    }

    public void OpenPopup(DiceData data, RectTransform targetRect)
    {
        if (hoverHintText != null) hoverHintText.SetActive(false);
        if (diceDesc == null) return;
        this.diceDesc.text = data.Desc;
        if (diceIcon != null) diceIcon.sprite = data.skin.GetSprite(1);
        if (diceName != null) diceName.text = data.abilityName;

        dicePopup.gameObject.SetActive(true);

        if (holdHint != null) holdHint.SetActive(true);
    }

    public void OpenPopup(BattleItemSo data, RectTransform targetRect)
    {
        if (hoverHintText != null) hoverHintText.SetActive(false);
        if (itemDesc == null) return;
        this.itemDesc.text = data.itemDesc;
        if (itemIcon != null) itemIcon.sprite = data.itemIcon;
        if (itemName != null) itemName.text = data.itemName;


        itemPopup.gameObject.SetActive(true);
        if (holdHint != null) holdHint.SetActive(true);
    }

    public void ClosePopup()
    {
        if(diceDesc != null) dicePopup.gameObject.SetActive(false);
        if(itemDesc != null) itemPopup.gameObject.SetActive(false);
        if (DescPopup != null) DescPopup.gameObject.SetActive(false);
        if(hoverHintText != null) hoverHintText.SetActive(true);
    }

    private void UpdateGold(int gold) => playerGold.text = $"{gold}";

    //private Vector2 CalcLocalPosPopup(RectTransform targetRect)
    //{
    //    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //        rootCanvas.GetComponent<RectTransform>(),
    //        screenPos,
    //        rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
    //        out Vector2 localPos
    //    );
    //    return localPos + new Vector2(targetRect.sizeDelta.x, 0);
    //}
}
