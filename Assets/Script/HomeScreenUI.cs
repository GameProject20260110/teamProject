using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreenUI : MonoBehaviour
{
    public static HomeScreenUI instance;
    public AudioClip clip;
    [Header("Button")]
    public Button StartBtn;

  
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
        StartBtn.onClick.AddListener(() => SceneController.instance.LoadGameScene());
    }

}
