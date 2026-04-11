using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GamePopupManager : MonoBehaviour
{
    public static GamePopupManager instance;

    [Header("ÁÖ»çÀ§ ÆË¾÷")]
    public RectTransform dicePopup;
    private TextMeshProUGUI diceDesc;

    [Header("±â¹Í ÆË¾÷")]
    public RectTransform gimmickPopup;
    private TextMeshProUGUI gimmickDesc;

    [Header("¼³¸í ÆË¾÷")]
    public ShopDescPopup DescPopup;

    public Canvas rootCanvas;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (dicePopup != null)
        {
            diceDesc = dicePopup.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (gimmickPopup != null)
        {
            gimmickDesc = gimmickPopup.GetComponentInChildren<TextMeshProUGUI>();
        }


    }

    private void Start()
    {
        ClosePopup();
    }

    public void DescOpenPopup(DiceData data)
    {
        DescPopup.gameObject.SetActive(true);
        DescPopup.UpdateUI(data);
    }

    public void DescOpenPopup(ItemSo data)
    {
        DescPopup.gameObject.SetActive(true);
        DescPopup.UpdateUI(data);
    }

    public void OpenPopup(DiceData data, RectTransform targetRect)
    {
        if (diceDesc == null) return;
        this.diceDesc.text = data.Desc;
        dicePopup.localPosition = CalcLocalPosPopup(targetRect);
        dicePopup.gameObject.SetActive(true);
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
        if (diceDesc != null) dicePopup.gameObject.SetActive(false);
        if (gimmickDesc != null) gimmickPopup.gameObject.SetActive(false);
    }

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

