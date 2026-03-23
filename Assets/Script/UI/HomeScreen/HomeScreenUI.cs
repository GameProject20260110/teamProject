using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreenUI : MonoBehaviour
{
    public static HomeScreenUI instance;

    [Header("Button")]
    [SerializeField] private Button StartBtn;
    [SerializeField] private Button optionBtn;

  
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
    }

    void Start()
    {
        AudioManager.instance.PlayBgm(AudioManager.Bgm.Home, true);
        StartBtn.onClick.AddListener(() => SceneController.instance.LoadShopScene());
        optionBtn.onClick.AddListener(() => SettingsManager.instance.ToggleSettings());
    }

}
