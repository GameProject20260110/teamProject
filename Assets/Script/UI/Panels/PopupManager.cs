using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public static PopupManager instance;

    [Header("주사위 팝업")]
    public RectTransform dicePopup;
    private TextMeshProUGUI diceDesc;

    [Header("아이템 팝업")]
    public RectTransform itemPopup;
    private TextMeshProUGUI itemDesc;

    [Header("기믹 팝업")]
    public RectTransform gimmickPopup;
    private TextMeshProUGUI gimmickDesc;

    [Header("설명 팝업")]
    public RectTransform DescPopup;
    public DescPopupContent Content;

    [Header("플레이어 정보")]
    public TextMeshProUGUI playerGold;
    public TextMeshProUGUI playerRound;

    [Header("기타")]
    public GameObject closePanel;
    public Button StartBtn;
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

        if (gimmickPopup != null)
        {
            gimmickDesc = gimmickPopup.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (StartBtn != null)
        {
            StartBtn.onClick.AddListener(() => SceneController.instance.LoadGameScene());
        }
    }

    private void Start()
    {
        ClosePopup();
        if (playerGold != null)
        {
            int gold = PlayerShopManager.instance != null && PlayerShopManager.instance.IsOpen ? 
                PlayerShopManager.instance.TempGold : PlayerManager.instance.gold;
            playerGold.text = gold.ToString();
            playerRound.text = PlayerManager.instance.currentRound.ToString();
        }
        if (PlayerShopManager.instance != null)
            PlayerShopManager.instance.OnGoldChanged += UpdateGold;
    }

    private void OnDestroy()
    {
        if (PlayerShopManager.instance != null)
            PlayerShopManager.instance.OnGoldChanged -= UpdateGold;
    }

    public void SetStatus()
    {
        playerGold.text = PlayerManager.instance.gold.ToString();
        playerRound.text = PlayerManager.instance.currentRound.ToString();
    }

    public void DescOpenPopup(DiceData data)
    {
        DescPopup.gameObject.SetActive(true);
        Content.UpdataInfo(data);
    }
    public void DescOpenPopup(ItemSo data)
    {
        DescPopup.gameObject.SetActive(true);
        Content.UpdataInfo(data);
    }

    public void OpenPopup(DiceData data, RectTransform targetRect)
    {
        if (diceDesc == null) return;
        this.diceDesc.text = data.Desc;
        dicePopup.localPosition = CalcLocalPosPopup(targetRect);
        dicePopup.gameObject.SetActive(true);
    }

    public void OpenPopup(ItemSo data, RectTransform targetRect)
    {
        if (itemDesc == null) return;
        this.itemDesc.text = data.itemDesc;
        itemPopup.localPosition = CalcLocalPosPopup(targetRect);
        itemPopup.gameObject.SetActive(true);
    }

    public void OpenGimmickPopup(GimmickSo data, RectTransform targetRect)
    {
        if (gimmickDesc == null) return;
        gimmickDesc.text = $"[{data.gimmickName}]\n\n{data.description}";

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            screenPos,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPos
        );
        gimmickPopup.localPosition = localPos + new Vector2(gimmickPopup.sizeDelta.x * -0.7f, targetRect.sizeDelta.y * 0.2f);
        gimmickPopup.gameObject.SetActive(true);
    }

    public void ClosePopup()
    {
        if(diceDesc != null) dicePopup.gameObject.SetActive(false);
        if(itemDesc != null) itemPopup.gameObject.SetActive(false);
        if(gimmickDesc != null) gimmickPopup.gameObject.SetActive(false);
    }

    private void UpdateGold(int gold) => playerGold.text = $"{gold}";

    private Vector2 CalcLocalPosPopup(RectTransform targetRect)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            screenPos,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPos
        );
        return localPos + new Vector2(targetRect.sizeDelta.x, 0);
    }
}
