using TMPro;
using UnityEngine;

public class DescManager : MonoBehaviour
{
    public static DescManager instance;

    [Header("°ñµå")]
    public TextMeshProUGUI textGold;

    [Header("¶ó¿îµå")]
    public TextMeshProUGUI currentRound;
    
    GameObject selectDesc;
    public bool descOn = false;

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
    }

    private void Start()
    {
        currentRound.text = "¶ó¿îµå Á¡¼ö: " + PlayerManager.instance.currentRound.ToString();
        textGold.text = "°ñµå: " + PlayerManager.instance.gold.ToString();
    }

    public void SelectDesc(GameObject selectDesc)
    {
        this.selectDesc = selectDesc;
        descOn = true;
    }

    public void DeSelectDesc()
    {
        if (selectDesc != null)
        {
            descOn = false;
            this.selectDesc.SetActive(false);
        }

    }

    public void BuyGold(int gold)
    {
        PlayerManager.instance.gold -= gold;
        textGold.text = "°ñµå: " + PlayerManager.instance.gold.ToString();
    }

    public void SellGold(int gold)
    {
        PlayerManager.instance.gold += gold;
        textGold.text = "°ñµå: " + PlayerManager.instance.gold.ToString();
    }
}
