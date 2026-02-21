using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public static PopupManager instance;

    public GameObject closePanel;
    
    public RectTransform dicePopup;
    private TextMeshProUGUI diceDesc;

    public RectTransform itemPopup;
    private TextMeshProUGUI itemDesc;

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
        rootCanvas = FindObjectOfType<Canvas>().rootCanvas;

        diceDesc = dicePopup.GetComponentInChildren<TextMeshProUGUI>();
        itemDesc = itemPopup.GetComponentInChildren<TextMeshProUGUI>();

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
                  
        //closePanel.SetActive(true);
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

        //closePanel.SetActive(true);
    }

    public void ClosePopup()
    {
        dicePopup.gameObject.SetActive(false);
        itemPopup.gameObject.SetActive(false);
        //closePanel.SetActive(false);
    }
}
