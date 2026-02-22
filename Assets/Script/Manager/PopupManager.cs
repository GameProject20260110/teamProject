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

    [Header("플레이어 정보")]
    public TextMeshProUGUI playerGold;
    public TextMeshProUGUI playerRound;

    [Header("기타")]
    public GameObject closePanel;
    private Canvas rootCanvas;

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
        rootCanvas = FindFirstObjectByType<Canvas>().rootCanvas;

        diceDesc = dicePopup.GetComponentInChildren<TextMeshProUGUI>();
        itemDesc = itemPopup.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        SetStatus();
    }

    public void SetStatus()
    {
        playerGold.text = PlayerManager.instance.gold.ToString();
        playerRound.text = PlayerManager.instance.currentRound.ToString();
    }


    public void OpenPopup(DiceData data, RectTransform targetRect)
    {
        this.diceDesc.text = data.Desc;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            screenPos,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPos
            );
       
        dicePopup.localPosition = localPos + new Vector2(targetRect.sizeDelta.x, 0);
        dicePopup.gameObject.SetActive(true);
    }

    public void OpenPopup(ItemSo data, RectTransform targetRect)
    {
        this.itemDesc.text = data.itemDesc;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            screenPos,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPos
            );

        itemPopup.localPosition = localPos + new Vector2(targetRect.sizeDelta.x, 0);
        itemPopup.gameObject.SetActive(true);
    }

    public void ClosePopup()
    {
        dicePopup.gameObject.SetActive(false);
        itemPopup.gameObject.SetActive(false);
    }

    public void BuyItems(int gold)
    {
        PlayerManager.instance.gold -= gold;
        playerGold.text = PlayerManager.instance.gold.ToString();
    }

    public void SellItems(int gold)
    {
        PlayerManager.instance.gold += gold;
        playerGold.text = PlayerManager.instance.gold.ToString();
    }
}
