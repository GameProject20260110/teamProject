using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreenUI : MonoBehaviour
{
    public static HomeScreenUI instance;
    public AudioClip clip;
    [Header("text")]
    public TextMeshProUGUI bestRoundtext;
    public TextMeshProUGUI bestScoretext;

  
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
        //bestRoundtext.text = "최고 라운드: " + PlayerManager.instance.bestRound.ToString();
        //bestScoretext.text = "최고 점수: " + PlayerManager.instance.bestScore.ToString();
        AudioManager.instance.PlayBgm(AudioManager.Bgm.Home, true);  
    }

}
